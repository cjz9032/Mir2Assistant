using Mir2Assistant.Common.Models;
using Mir2Assistant.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Mir2Assistant.Common.Functions;
/// <summary>
/// 技能
/// </summary>
public static class SkillFunction
{
    /// <summary>
    /// 读取角色技能列表
    /// </summary>
    /// <param name="gameInstance"></param>
    public static void ReadSkills(MirGameInstanceModel gameInstance)
    {
        Task.Run(() =>
        {
            if(GameState.MirConfig["技能基址"] == 0) return;
            var memoryUtils = gameInstance!.memoryUtils!;
            var count = memoryUtils.ReadToInt8(memoryUtils.GetMemoryAddress(GameState.MirConfig["技能基址"], 8));
            if(count == gameInstance.Skills.Count) return;
            gameInstance.Skills.Clear();
            for (int i = 0; i < count; i++)
            {
                var addr = memoryUtils.GetMemoryAddress(GameState.MirConfig["技能基址"], 0x4, i * 0x4);
                var nameLength = (byte)memoryUtils.ReadToChar(memoryUtils.GetMemoryAddress(addr, 0xA));

                var skill = new SkillModel
                {
                    Id = memoryUtils.ReadToInt8(memoryUtils.GetMemoryAddress(addr, 0x8)),
                    Addr = memoryUtils.ReadToInt(addr),
                    Name = memoryUtils.ReadToString(memoryUtils.GetMemoryAddress(addr, 0xA+1), nameLength),
                    points = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(addr, 0x4)),
                    level = memoryUtils.ReadToInt8(memoryUtils.GetMemoryAddress(addr, 0x1)), // 
                    maxPoints = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(addr, 0x28)) // 0x24??
                };
                gameInstance.Skills.Add(skill);
            }
        });
    }



}
