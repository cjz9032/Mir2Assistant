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
        var bagBaseAddr = (int)GameState.MirConfig["装备基址"];
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

        status.Name = memoryUtils.ReadToDelphiUnicode(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], GameState.MirConfig["人物NAME偏移"], 0));
        status.MapName = memoryUtils.ReadToDelphiUnicode(memoryUtils.GetMemoryAddress(GameState.MirConfig["地图基址"], 0));
        status.MapId = memoryUtils.ReadToDelphiUnicode(memoryUtils.GetMemoryAddress(GameState.MirConfig["地图ID基址"], 0x2AE8C, 0));
        status.X = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], GameState.MirConfig["人物X偏移"]));
        status.Y = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], GameState.MirConfig["人物Y偏移"]));
        status.CurrentHP = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], GameState.MirConfig["人物CurrentHP偏移"]));
        status.MaxHP = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], GameState.MirConfig["人物MAXHP偏移"]));
        status.CurrentMP = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], GameState.MirConfig["人物CurrentMP偏移"]));
        status.MaxMP = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], GameState.MirConfig["人物MAXMP偏移"]));
        status.Level = memoryUtils.ReadToInt8(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], GameState.MirConfig["人物LEVEL偏移"]));
        status.groupMemCount = memoryUtils.ReadToInt8(memoryUtils.GetMemoryAddress(GameState.MirConfig["MIR_GROUP_MEMBER_ADDR"], 0x30));
        status.allowGroup = memoryUtils.ReadToInt8(memoryUtils.GetMemoryAddress(GameState.MirConfig["MIR_GROUP_ALLOW_ADDR"])) == 1;
        if(GameState.MirConfig["金地"] != 0){
            status.coin = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(GameState.MirConfig["金地"], 0 ,GameState.MirConfig["金币偏移"]));
        }
    }

    public static void ReadChats(MirGameInstanceModel gameInstance, bool force = false)
    {
        if (gameInstance.isRefreshing && !force)
        {
            return;
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var chats = new List<string>();
        var memoryUtils = gameInstance!.memoryUtils!;
        var chatAddr = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(GameState.MirConfig["TDrawScreen"], 0x1C));
        var count = memoryUtils.ReadToInt(chatAddr + 0x30);
        var chatArrayAddr = memoryUtils.ReadToInt(chatAddr + 0x2C);
        int takeCount = Math.Min(count, 100);
        int startIndex = count - takeCount;

        for (int i = startIndex; i < count; i++)
        {
            var itemAddr = memoryUtils.GetMemoryAddress(memoryUtils.ReadToInt(chatArrayAddr + i * 0x8));
            var name = memoryUtils.ReadToDelphiUnicode(itemAddr);
            chats.Add(name);
        }
        gameInstance.chats = chats;

        stopwatch.Stop();
        Console.WriteLine($"ReadChats 耗时: {stopwatch.ElapsedMilliseconds}ms");
    }


    public static void FastUpdateXY(MirGameInstanceModel gameInstance)
    {
        var status = gameInstance.CharacterStatus!;
        var memoryUtils = gameInstance.memoryUtils!;
        status.X = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], GameState.MirConfig["人物X偏移"]));
        status.Y = memoryUtils.ReadToShort(memoryUtils.GetMemoryAddress(GameState.MirConfig["角色基址"], GameState.MirConfig["人物Y偏移"]));
    }

    public static void AddChat(MirGameInstanceModel gameInstance, string chat)
    {
        // 考虑到很男识别 就先不要识别什么时候完成, 自行用clear自己识别
        nint[] data = Utils.StringUtils.GenerateCompactStringData(chat);
        SendMirCall.Send(gameInstance, 9200, data);
    }

    public static void ClearChats(MirGameInstanceModel gameInstance)
    {
        SendMirCall.Send(gameInstance, 9201, new nint[] { 0 });
    }


    public static void AdjustAttackSpeed(MirGameInstanceModel gameInstance, int attackSpeed)
    {
        var memoryUtils = gameInstance.memoryUtils!;
        memoryUtils.WriteInt(memoryUtils.GetMemoryAddress(GameState.MirConfig["攻速基址"]), attackSpeed);
    }
     public static void AdjustMoveSpeed(MirGameInstanceModel gameInstance, int attackSpeed)
    {
        var memoryUtils = gameInstance.memoryUtils!;
        memoryUtils.WriteInt(memoryUtils.GetMemoryAddress(GameState.MirConfig["移速基址"]), attackSpeed);
    }


}

