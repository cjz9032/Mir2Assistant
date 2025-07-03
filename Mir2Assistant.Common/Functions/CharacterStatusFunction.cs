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
        status.Name = memoryUtils.ReadToString(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0, 0x1D4, 0));
        status.MapName = memoryUtils.ReadToString(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0x10, 0));
        status.X = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0, 0x08));
        status.Y = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0, 0x0C));
        status.CurrentHP = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0, 0x20C));
        status.MaxHP = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0, 0x214));
        status.CurrentMP = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0, 0x210));
        status.MaxMP = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0, 0x218));
    }
}

