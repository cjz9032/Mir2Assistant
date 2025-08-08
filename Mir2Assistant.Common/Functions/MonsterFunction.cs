using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.Utils;
using Mir2Assistant.Utils;
using System;
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

        for (int i = 0; i < monsterCount; i++)
        {
            bool isNew = false;
            var monsterAddr = memoryUtils.ReadToInt(monsterArrayAddr + i * 0x4);
            var id = memoryUtils.ReadToInt(monsterAddr +  GameState.MirConfig["怪物ID偏移"]);
            gameInstance.Monsters.TryGetValue(id, out MonsterModel? monster);
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

            monster.X = memoryUtils.ReadToShort(monsterAddr + GameState.MirConfig["怪物X偏移"]);
            monster.Y = memoryUtils.ReadToShort(monsterAddr + GameState.MirConfig["怪物Y偏移"]);

            monster.CurrentHP = memoryUtils.ReadToShort(monsterAddr + GameState.MirConfig["怪物CHP偏移"]);
            monster.MaxHP = memoryUtils.ReadToShort(monsterAddr + GameState.MirConfig["怪物MHP偏移"]);

            monster.CurrentMP = memoryUtils.ReadToShort(monsterAddr +  GameState.MirConfig["怪物CMP偏移"]);
            monster.MaxMP = memoryUtils.ReadToShort(monsterAddr +  GameState.MirConfig["怪物MMP偏移"]);

            monster.Level = memoryUtils.ReadToInt8(monsterAddr + GameState.MirConfig["怪物LV偏移"]);

            if (isNew)
            {
                gameInstance.Monsters.TryAdd(id, monster);
            }
            monster.isDead = memoryUtils.ReadToInt8(monsterAddr +  GameState.MirConfig["怪物DEAD偏移"]) > 0;
            monster.isButched = memoryUtils.ReadToInt8(monsterAddr +  GameState.MirConfig["怪物BUTCHED偏移"]) > 0;
            monster.Appr = memoryUtils.ReadToInt8(monsterAddr + GameState.MirConfig["怪物APPR偏移"]);
        }
        foreach (var item in gameInstance.Monsters.Values.Where(o => o.UpdateId != gameInstance.MonstersUpdateId))
        {
            gameInstance.Monsters.TryRemove(item.Id, out MonsterModel? m);
        }
        gameInstance.IsReadingMonsters = false;
    }

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
