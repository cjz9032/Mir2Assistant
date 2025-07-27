using Mir2Assistant.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mir2Assistant.Common.Functions;

public static class CharacterStatusFunction
{
    public static void GetUsedItemInfo(MirGameInstanceModel gameInstance, bool force = false)
    {
        if (gameInstance.isRefreshing && !force)
        {
            return;
        }
        var bagBaseAddr = gameInstance!.memoryUtils!.ReadToInt(0x00679940);
        ItemFunction.ReadItems(gameInstance, bagBaseAddr, gameInstance.CharacterStatus!.useItems);
    }

    public static void GetInfo(MirGameInstanceModel gameInstance, bool force = false)
    {
        if (gameInstance.isRefreshing && !force)
        {
            return;
        }
        var status = gameInstance.CharacterStatus!;
        var memoryUtils = gameInstance.memoryUtils!;


        status.Name = memoryUtils.ReadToDelphiUnicode(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], 0x34, 0));
        status.MapName = memoryUtils.ReadToDelphiUnicode(memoryUtils.GetMemoryAddress(GameState.MirConfig["地图基址"], 0));
        status.MapId = memoryUtils.ReadToDelphiUnicode(memoryUtils.GetMemoryAddress(GameState.MirConfig["地图ID基址"], 0x2AE8C, 0));
        status.X = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], 0x8));
        status.Y = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], 0xA));
        status.CurrentHP = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], 0x48));
        status.MaxHP = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], 0x4C));
        status.CurrentMP = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], 0x4A));
        status.MaxMP = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], 0x4E));
        status.Level = memoryUtils.ReadToInt8(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], 0x3C));
        status.groupMemCount = memoryUtils.ReadToInt8(memoryUtils.GetMemoryAddress(0x7563CC, 0x30));
        status.allowGroup = memoryUtils.ReadToInt8(memoryUtils.GetMemoryAddress(0x7563C8)) == 1;
    }

    public static void FastUpdateXY(MirGameInstanceModel gameInstance)
    {
        var status = gameInstance.CharacterStatus!;
        var memoryUtils = gameInstance.memoryUtils!;
        status.X = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], 0x8));
        status.Y = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], 0xA));
    }
    
}

