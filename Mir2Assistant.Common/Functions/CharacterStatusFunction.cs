using Mir2Assistant.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mir2Assistant.Common.Functions;

public static class CharacterStatusFunction
{
    public static void GetInfo(MirGameInstanceModel gameInstance)
    {
        var status = gameInstance.CharacterStatus!;
        var memoryUtils = gameInstance.MemoryUtils!;
        status.Name = memoryUtils.ReadToString(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0x4, 0));
        status.MapName = memoryUtils.ReadToString(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0x10, 0));
        status.X = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0, 0x12c));
        status.Y = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0, 0x130));
        status.CurrentHP = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0, 0xac));
        status.MaxHP = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0, 0x6c));
        status.CurrentMP = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0, 0x94));
        status.MaxMP = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0, 0x84));
        status.GradeZS = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0, gameInstance.MirConfig["转生偏移"]));
    }
}

