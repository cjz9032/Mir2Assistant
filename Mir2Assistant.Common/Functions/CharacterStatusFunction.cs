using Mir2Assistant.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mir2Assistant.Common.Functions;

public static class CharacterStatusFunction
{
    public static void GetUsedItemInfo(MirGameInstanceModel gameInstance)
    {
        var bagBaseAddr = gameInstance!.MemoryUtils!.ReadToInt(0x00679940);
        ItemFunction.ReadItems(gameInstance, bagBaseAddr, gameInstance.CharacterStatus!.useItems);
    }

    public static void GetInfo(MirGameInstanceModel gameInstance)
    {
        var status = gameInstance.CharacterStatus!;
        var memoryUtils = gameInstance.MemoryUtils!;



        status.Name = memoryUtils.ReadToString(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0, 0x1D4, 0));
        status.MapName = memoryUtils.ReadToDelphiUnicode(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["地图基址"], 0));
        status.MapId = memoryUtils.ReadToDelphiUnicode(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["地图ID基址"], 0x2AE8C, 0));
        status.X = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0x8));
        status.Y = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0xA));
        status.CurrentHP = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0x48));
        status.MaxHP = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"],0x4C));
        status.CurrentMP = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0x4A));
        status.MaxMP = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(gameInstance.MirConfig["角色基址"], 0x4E));
        GetUsedItemInfo(gameInstance);
        
    }
}

