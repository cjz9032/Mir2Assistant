using System.Diagnostics;
using Mir2Assistant.Common.Models;
using Serilog;

namespace Mir2Assistant.Common.Functions
{
    public static class ItemFunction
    {
        public static void ReadItems(MirGameInstanceModel gameInstance, int baseAddr, List<ItemModel> targetItems)
        {
            Task.Run(() =>
            {
                //if (gameInstance.IsReadingItems)
                //{
                //    return;
                //}
                gameInstance.IsReadingItems = true;
                var memoryUtils = gameInstance!.memoryUtils!;
                int itemSize = (int)GameState.MirConfig["物品SIZE"]; // 每个item占80字节

                for (int i = 0; i < targetItems.Count; i++)
                {
                    var item = targetItems[i];
                    var itemAddr = baseAddr + i * itemSize;
                    byte nameLength = memoryUtils.ReadToInt8(itemAddr);
                    item.IsEmpty = nameLength == 0;
                    
                    if (!item.IsEmpty)
                    {
                        item.Id = memoryUtils.ReadToInt(itemAddr + (int)GameState.MirConfig["物品ID"]);
                        if (nameLength > 0)
                        {
                            item.Name = memoryUtils.ReadToString(itemAddr + 1, nameLength);
                        }
                        else
                        {
                            item.Name = string.Empty;
                        }
                        if (GameState.MirConfig["物品极品点"] > 0)
                        {
                            item.GodPts = memoryUtils.ReadToInt(itemAddr + GameState.MirConfig["物品极品点"]);
                        }
                        else
                        {
                            item.GodPts = 0;
                        }
                        item.IsGodly = item.GodPts > 0;
                        item.Duration = memoryUtils.ReadToShort(itemAddr + GameState.MirConfig["物品持久"])/100;
                        item.MaxDuration = memoryUtils.ReadToShort(itemAddr + GameState.MirConfig["物品最大持久"])/100;
                        item.stdMode = memoryUtils.ReadToInt8(itemAddr + GameState.MirConfig["物品MODE"]);
                        item.addr = itemAddr;
                        item.reqType = memoryUtils.ReadToInt8(itemAddr +  GameState.MirConfig["物品ReqType"]);
                        item.reqPoints = memoryUtils.ReadToInt8(itemAddr + GameState.MirConfig["物品ReqPts"] );

                        
                    }
                    else
                    {
                        item.Id = 0;
                        item.Name = string.Empty;
                    }
                }

                gameInstance.IsReadingItems = false;
            });
        }
        public static void ReadDrops(MirGameInstanceModel gameInstance, bool force = false)
        {   
            if (gameInstance.isRefreshing && !force)
            {
                return;
            }
            // drops
            // -- looks +8
            var memoryUtils = gameInstance!.memoryUtils!;
            var dropsAddr = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(GameState.MirConfig["地物数组"]));
            var count = memoryUtils.ReadToInt(dropsAddr + 0x8);
            var dropsArrayAddr = memoryUtils.ReadToInt(dropsAddr + 0x4);
            ++gameInstance.DropsItemsUpdateId;

            for (int i = 0; i < count; i++)
            {
                bool isNew = false;
                var itemAddr = memoryUtils.ReadToInt(dropsArrayAddr + i * 0x4);
                var id = memoryUtils.ReadToInt(itemAddr);
                gameInstance.DropsItems.TryGetValue(id, out DropItemModel? item);
                if (item == null)
                {
                    item = new DropItemModel();
                    isNew = true;
                }
                item.UpdateId = gameInstance.DropsItemsUpdateId;
                item.Id = id;
                byte nameLength = memoryUtils.ReadToInt8(itemAddr+GameState.MirConfig["地物NAME偏移"]);
                item.Name = memoryUtils.ReadToString(itemAddr + GameState.MirConfig["地物NAME偏移"] + 1, nameLength);
                if(GameState.MirConfig["地物极品点"] > 0){
                    item.GodPts = memoryUtils.ReadToInt8(itemAddr + GameState.MirConfig["地物极品点"]);
                }else{
                    item.GodPts = 0;
                }
                item.IsGodly = item.GodPts > 0;

                item.X = memoryUtils.ReadToShort(itemAddr + GameState.MirConfig["地物X"]);
                item.Y = memoryUtils.ReadToShort(itemAddr + GameState.MirConfig["地物Y"]);
                if (isNew)
                {
                    gameInstance.DropsItems.TryAdd(id, item);
                }
            }
            foreach (var item in gameInstance.DropsItems.Values.Where(o => o.UpdateId != gameInstance.DropsItemsUpdateId))
            {
                gameInstance.DropsItems.TryRemove(item.Id, out DropItemModel? m);
            }

        }

   
        public static void ReadBag(MirGameInstanceModel gameInstance, bool force = false)
        {
            if (gameInstance.isRefreshing && !force)
            {
                return;
            }
            // var sw = Stopwatch.StartNew();

            ReadItems(gameInstance, (int)GameState.MirConfig["背包基址"] + ((int)GameState.MirConfig["物品SIZE"]*6), gameInstance.Items);
            ReadItems(gameInstance, (int)GameState.MirConfig["背包基址"] , gameInstance.QuickItems);
            // sw.Stop();
            // Log.Debug($"读取背包耗时: {sw.ElapsedMilliseconds}ms, 物品数量: {gameInstance.Items.Count}");
        }

    

        public static void Pickup(MirGameInstanceModel gameInstance)
        {
            SendMirCall.Send(gameInstance, 3031, new nint[] { });
        }
    }
}