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
    public static void ReadMonster(MirGameInstanceModel gameInstance)
    {
        Task.Run(() =>
        {
            if (gameInstance.IsReadingMonsters)
            {
                return;
            }
            gameInstance.IsReadingMonsters = true;
            //var Monsters = new List<MonsterModel>();
            var memoryUtils = gameInstance!.MemoryUtils!;
            var monstersAddr = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["怪物数组"], 0x34C8));
            var monsterCount = memoryUtils.ReadToInt(monstersAddr + 0x8);
            var monsterArrayAddr = memoryUtils.ReadToInt(monstersAddr + 0x4);
            byte flag = 0;
            ++gameInstance.MonstersUpdateId;

            for (int i = 0; i < monsterCount; i++)
            {
                bool isNew = false;
                var monsterAddr = memoryUtils.ReadToInt(monsterArrayAddr + i * 0x4);
                var id = memoryUtils.ReadToInt(monsterAddr + 0x4);
                gameInstance.Monsters.TryGetValue(id, out MonsterModel? monster);
                if (monster == null)
                {
                    monster = new MonsterModel();
                    isNew = true;
                }
                monster.UpdateId = gameInstance.MonstersUpdateId;
                monster.Id = id;
                monster.Type = monster.Type ?? memoryUtils.ReadToInt8(monsterAddr + 0x18); // todo confirm
                monster.Addr = monsterAddr;
                if (string.IsNullOrEmpty(monster.Name)) {
                    var baseAd = memoryUtils.GetMemoryAddress(monsterAddr + 0x34 - 1);
                        var nestedRef = memoryUtils.GetMemoryAddress(baseAd+1, 0);

                        var len2 = memoryUtils.ReadToInt(nestedRef - 4) * 2;

                        monster.Name = memoryUtils.ReadToUnicode(nestedRef, len2);
                }
                    
                // todo side effect
                // memoryUtils.WriteShort(memoryUtils.GetMemoryAddress(monsterAddr + 0x158), 1);

                if (monster.TypeStr != "NPC" || monster.X == null)               {
                    monster.X = memoryUtils.ReadToShort(monsterAddr + 0x08);
                    monster.Y = memoryUtils.ReadToShort(monsterAddr + 0x0A);
                }
                //MonsterModel.Guild = memoryUtils.ReadToString(memoryUtils.GetMemoryAddress(monsterAddr + 0x44, 0));
                monster.Flag = flag;

                if (monster.Name == gameInstance.CharacterStatus!.Name)
                {
                    monster.Flag = 1;
                    flag = 2;
                }
                if (isNew)
                {
                    gameInstance.Monsters.TryAdd(id, monster);
                }
                monster.isDead = memoryUtils.ReadToInt8(monsterAddr + 0x28) > 0;
                monster.isButched = memoryUtils.ReadToInt8(monsterAddr + 0x29) > 0;
                monster.Appr = memoryUtils.ReadToInt8(monsterAddr + 0x16);
            }
            foreach (var item in gameInstance.Monsters.Values.Where(o => o.UpdateId != gameInstance.MonstersUpdateId))
            {
                gameInstance.Monsters.TryRemove(item.Id, out MonsterModel? m);
            }
            gameInstance.IsReadingMonsters = false;
        });
    }

    /// <summary>
    /// 锁定怪物，相当物游戏中鼠标指向某个怪
    /// </summary>
    public static void LockMonster(MirGameInstanceModel gameInstance, int monsterAddr)
    {
        var memoryUtils = gameInstance!.MemoryUtils!;
        memoryUtils.WriteInt(gameInstance.MirConfig["存怪参数"], monsterAddr);
    }


    /// <summary>
    /// 砍怪
    /// </summary>
    /// <param name="gameInstance"></param>
    /// <param name="monsterAddr"></param>
    public static void SlayingMonster(MirGameInstanceModel gameInstance, int monsterAddr)
    {
        var memoryUtils = gameInstance!.MemoryUtils!;
        memoryUtils.WriteInt(gameInstance.MirConfig["打怪基址"], monsterAddr);
    }

}
