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
            var Monsters = new List<MonsterModel>();
            var memoryUtils = gameInstance!.MemoryUtils!;
            var monstersAddr = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], -0x4, 0x3528));
            var monsterCount = memoryUtils.ReadToInt(monstersAddr + 0x8);
            var monsterArrayAddr = memoryUtils.ReadToInt(monstersAddr + 0x4);
            byte flag = 0;
            for (int i = 0; i < monsterCount; i++)
            {
                var MonsterModel = new MonsterModel();

                var monsterAddr = memoryUtils.ReadToInt(monsterArrayAddr + i * 0x4);
                MonsterModel.Addr = monsterAddr;
                MonsterModel.Type = memoryUtils.ReadToShort(monsterAddr + 0x20);
                MonsterModel.X = memoryUtils.ReadToShort(monsterAddr + 8);
                MonsterModel.Y = memoryUtils.ReadToShort(monsterAddr + 10);
                MonsterModel.Name = memoryUtils.ReadToString(memoryUtils.GetMemoryAddress(monsterAddr + 0x44, 0));
                MonsterModel.Flag = flag;

                if (MonsterModel.Name == gameInstance.CharacterStatus!.Name)
                {
                    MonsterModel.Flag = 1;
                    flag = 2;
                }
                Monsters.Add(MonsterModel);
            }
            gameInstance.Monsters.Clear();
            Monsters.ForEach(x => gameInstance.Monsters.Add(x));
            gameInstance.IsReadingMonsters = false;
        });
    }

    /// <summary>
    /// 锁定怪物，相当物游戏中鼠标指向某个怪
    /// </summary>
    public static void LockMonster(MirGameInstanceModel gameInstance, int skillAddr)
    {
        var memoryUtils = gameInstance!.MemoryUtils!;
        memoryUtils.WriteInt(gameInstance.MirConfig["存怪参数"], skillAddr);
    }


    /// <summary>
    /// 砍怪
    /// </summary>
    /// <param name="gameInstance"></param>
    /// <param name="monsterAddr"></param>
    public static void SlayingMonster(MirGameInstanceModel gameInstance, int monsterAddr)
    {
        var memoryUtils = gameInstance!.MemoryUtils!;
        memoryUtils.WriteInt(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["打怪基址"], -0xc), monsterAddr);
    }

}
