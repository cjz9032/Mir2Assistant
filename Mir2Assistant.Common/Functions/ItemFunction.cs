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
                int itemSize = 0x80; // 每个item占80字节

                for (int i = 0; i < targetItems.Count; i++)
                {
                    var item = targetItems[i];
                    var itemAddr = baseAddr + i * itemSize;
                    byte nameLength = memoryUtils.ReadToInt8(itemAddr);
                    item.IsEmpty = nameLength == 0;
                    
                    if (!item.IsEmpty)
                    {
                        item.Id = memoryUtils.ReadToInt(itemAddr + 0x74);
                        if (nameLength > 0)
                        {
                            item.Name = memoryUtils.ReadToString(itemAddr + 1, nameLength);
                        }
                        else
                        {
                            item.Name = string.Empty;
                        }
                        item.GodPts = memoryUtils.ReadToInt(itemAddr + 0x7C);
                        item.IsGodly = item.GodPts > 0;
                        item.Duration = memoryUtils.ReadToShort(itemAddr + 0x78)/100;
                        item.MaxDuration = memoryUtils.ReadToShort(itemAddr + 0x7A)/100;
                        item.stdMode = memoryUtils.ReadToInt8(itemAddr + 0xF);
                        item.addr = itemAddr;
                        item.reqType = memoryUtils.ReadToInt8(itemAddr + 0x24);
                        item.reqPoints = memoryUtils.ReadToInt8(itemAddr + 0x25);

                        
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
            var dropsAddr = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(0x7524D0));
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
                byte nameLength = memoryUtils.ReadToInt8(itemAddr+0x24);
                item.Name = memoryUtils.ReadToString(itemAddr + 0x25, nameLength);
                item.GodPts = memoryUtils.ReadToInt8(itemAddr + 0x54);
                item.IsGodly = item.GodPts > 0;

                item.X = memoryUtils.ReadToShort(itemAddr + 0x4);
                item.Y = memoryUtils.ReadToShort(itemAddr + 0x6);
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
            ReadItems(gameInstance, 0x007531E8, gameInstance.Items);
            ReadItems(gameInstance, 0x007531E8 - 0x80 * 6, gameInstance.QuickItems);
            // sw.Stop();
            // Log.Debug($"读取背包耗时: {sw.ElapsedMilliseconds}ms, 物品数量: {gameInstance.Items.Count}");
        }

    

        public static void Pickup(MirGameInstanceModel gameInstance)
        {
            SendMirCall.Send(gameInstance, 3031, new nint[] { });
        }
    }
}