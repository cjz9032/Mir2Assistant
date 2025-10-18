using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.Utils;
using Mir2Assistant.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mir2Assistant.Common.Functions;
/// <summary>
/// 读怪、砍怪
/// </summary>
public static class MonsterFunction
{

    /// <summary>
    /// 读怪
    /// </summary>
    public static void ReadMonster(MirGameInstanceModel gameInstance, bool force = false)
    {
        if (gameInstance.isRefreshing && !force)
        {
            return;
        }
     
        if (gameInstance.IsReadingMonsters)
        {
            return;
        }
        gameInstance.IsReadingMonsters = true;
        //var Monsters = new List<MonsterModel>();
        var memoryUtils = gameInstance!.memoryUtils!;
        var monstersAddr = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(GameState.MirConfig["怪物数组"], GameState.MirConfig["怪物数组偏移"]));
        var monsterCount = memoryUtils.ReadToInt(monstersAddr + 0x8);
        var monsterArrayAddr = memoryUtils.ReadToInt(monstersAddr + 0x4);
        ++gameInstance.MonstersUpdateId;

        var teamNames = GameState.GameInstances.Select(t => t.AccountInfo.CharacterName);

        for (int i = 0; i < monsterCount; i++)
        {
            bool isNew = false;
            var monsterAddr = memoryUtils.ReadToInt(monsterArrayAddr + i * 0x4);
            var id = memoryUtils.ReadToInt(monsterAddr + GameState.MirConfig["怪物ID偏移"]);
            gameInstance.Monsters.TryGetValue(id, out MonsterModel? monster);
            
            // 记录旧位置用于位置索引更新
            long? oldPositionKey = null;
            if (monster != null)
            {
                oldPositionKey = ((long)monster.X << 32) | (uint)monster.Y;
            }
            
            if (monster == null)
            {
                monster = new MonsterModel();
                isNew = true;
            }
            // name为空说明无效
            var myName = memoryUtils.ReadToDelphiUnicode(memoryUtils.GetMemoryAddress(monsterAddr + GameState.MirConfig["怪物NAME偏移"], 0));
            if (string.IsNullOrEmpty(myName))
            {
                // new回被过滤, 旧也会因旧id销毁
                continue;
            }

            monster.UpdateId = gameInstance.MonstersUpdateId;
            monster.Id = id;
            monster.Type = monster.Type ?? memoryUtils.ReadToInt8(monsterAddr +  GameState.MirConfig["怪物TYPE偏移"]); 
            monster.Addr = monsterAddr;
            monster.Name = myName;

            var newX = memoryUtils.ReadToShort(monsterAddr + GameState.MirConfig["怪物X偏移"]);
            var newY = memoryUtils.ReadToShort(monsterAddr + GameState.MirConfig["怪物Y偏移"]);
            
            // 检查位置是否变化
            bool positionChanged = monster.X != newX || monster.Y != newY;
            
            monster.X = newX;
            monster.Y = newY;

            monster.CurrentHP = memoryUtils.ReadToShort(monsterAddr + GameState.MirConfig["怪物CHP偏移"]);
            monster.MaxHP = memoryUtils.ReadToShort(monsterAddr + GameState.MirConfig["怪物MHP偏移"]);

            monster.CurrentMP = memoryUtils.ReadToShort(monsterAddr + GameState.MirConfig["怪物CMP偏移"]);
            monster.MaxMP = memoryUtils.ReadToShort(monsterAddr + GameState.MirConfig["怪物MMP偏移"]);

            monster.Level = memoryUtils.ReadToInt8(monsterAddr + GameState.MirConfig["怪物LV偏移"]);
            // monster.state = memoryUtils.ReadToInt(monsterAddr + GameState.MirConfig["MonsState偏移"]);

            monster.isTeams = teamNames.Any(n => monster.Name.Contains(n));
            monster.isTeamMem = monster.isTeams && monster.TypeStr == "玩家";
            monster.isSelf = monster.isTeamMem && monster.Name == gameInstance.AccountInfo.CharacterName;
            monster.isTeamMons = monster.isTeams && monster.TypeStr == "(怪)";
            monster.isMyMons = monster.isTeamMons && monster.Name.Contains(gameInstance.AccountInfo.CharacterName);

            if (isNew)
            {
                gameInstance.Monsters.TryAdd(id, monster);
            }
            
            // 更新位置索引
            var newPositionKey = ((long)monster.X << 32) | (uint)monster.Y;
            
            // 如果位置变化了，需要更新索引
            if (!isNew && positionChanged && oldPositionKey.HasValue)
            {
                // 从旧位置移除
                if (gameInstance.MonstersByPosition.TryGetValue(oldPositionKey.Value, out var oldList))
                {
                    oldList.Remove(monster);
                    if (oldList.Count == 0)
                    {
                        gameInstance.MonstersByPosition.TryRemove(oldPositionKey.Value, out _);
                    }
                }
            }
            
            // 添加到新位置（新怪物或位置变化的怪物）
            if (isNew || positionChanged)
            {
                gameInstance.MonstersByPosition.AddOrUpdate(newPositionKey,
                    new List<MonsterModel> { monster },
                    (key, existingList) =>
                    {
                        if (!existingList.Contains(monster))
                        {
                            existingList.Add(monster);
                        }
                        return existingList;
                    });
            }
            monster.isDead = memoryUtils.ReadToInt8(monsterAddr + GameState.MirConfig["怪物DEAD偏移"]) > 0;
            monster.isButched = memoryUtils.ReadToInt8(monsterAddr + GameState.MirConfig["怪物BUTCHED偏移"]) > 0;
            monster.Appr = memoryUtils.ReadToInt8(monsterAddr + GameState.MirConfig["怪物APPR偏移"]);
        }
        // 清理过期怪物
        foreach (var item in gameInstance.Monsters.Values.Where(o => o.UpdateId != gameInstance.MonstersUpdateId))
        {
            // 从位置索引中移除
            var positionKey = ((long)item.X << 32) | (uint)item.Y;
            if (gameInstance.MonstersByPosition.TryGetValue(positionKey, out var list))
            {
                list.Remove(item);
                if (list.Count == 0)
                {
                    gameInstance.MonstersByPosition.TryRemove(positionKey, out _);
                }
            }
            
            gameInstance.Monsters.TryRemove(item.Id, out MonsterModel? m);
        }
        gameInstance.IsReadingMonsters = false;
    }

    /// <summary>
    /// 根据坐标快速获取该位置的怪物列表
    /// </summary>
    /// <param name="gameInstance">游戏实例</param>
    /// <param name="x">X坐标</param>
    /// <param name="y">Y坐标</param>
    /// <returns>该位置的怪物列表，如果没有则返回空列表</returns>
    public static List<MonsterModel> GetMonstersByPosition(ConcurrentDictionary<long, List<MonsterModel>> MonstersByPosition, int x, int y)
    {
        var positionKey = ((long)x << 32) | (uint)y;
        return MonstersByPosition.TryGetValue(positionKey, out var monsters) ? monsters : new List<MonsterModel>();
    }

    /// <summary>
    /// 获取指定范围内的活着的怪物
    /// </summary>
    /// <param name="gameInstance">游戏实例</param>
    /// <param name="centerX">中心X坐标</param>
    /// <param name="centerY">中心Y坐标</param>
    /// <param name="range">范围</param>
    // /// <returns>范围内活着的怪物列表</returns>
    // public static List<MonsterModel> GetAliveMonsters(MirGameInstanceModel gameInstance, int centerX, int centerY, int range)
    // {
    //     return GetMonstersInRange(gameInstance, centerX, centerY, range)
    //         .Where(m => m.stdAliveMon)
    //         .ToList();
    // }

    /// <summary>
    /// 锁定怪物，相当物游戏中鼠标指向某个怪
    /// </summary>
    public static void LockMonster(MirGameInstanceModel gameInstance, int monsterAddr)
    {
        var memoryUtils = gameInstance!.memoryUtils!;
        memoryUtils.WriteInt(GameState.MirConfig["存怪参数"], monsterAddr);
    }


    /// <summary>
    /// 砍怪
    /// </summary>
    /// <param name="gameInstance"></param>
    /// <param name="monsterAddr"></param>
    public static void SlayingMonster(MirGameInstanceModel gameInstance, int monsterAddr)
    {
        var memoryUtils = gameInstance!.memoryUtils!;
        memoryUtils.WriteInt(GameState.MirConfig["存怪参数"], monsterAddr);
    }
    public static void SlayingMonsterCancel(MirGameInstanceModel gameInstance)
    {
        var memoryUtils = gameInstance!.memoryUtils!;
        memoryUtils.WriteInt(GameState.MirConfig["存怪参数"], 0);
    }
}
