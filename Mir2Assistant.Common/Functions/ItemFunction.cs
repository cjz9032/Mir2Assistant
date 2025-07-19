using Mir2Assistant.Common.Models;

namespace Mir2Assistant.Common.Functions
{
    public static class ItemFunction
    {
        public static void ReadItems(MirGameInstanceModel gameInstance, int baseAddr, List<ItemModel> targetItems)
        {
            Task.Run(() =>
            {
                if (gameInstance.IsReadingItems)
                {
                    return;
                }
                gameInstance.IsReadingItems = true;
                var memoryUtils = gameInstance!.MemoryUtils!;
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
                        item.IsGodly = memoryUtils.ReadToInt(itemAddr + 0x7C) == 1;
                        item.Duration = memoryUtils.ReadToShort(itemAddr + 0x78)/100;
                        item.MaxDuration = memoryUtils.ReadToShort(itemAddr + 0x7A)/100;
                        item.stdMode = memoryUtils.ReadToInt8(itemAddr + 0xF);
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

        public static void ReadBag(MirGameInstanceModel gameInstance)
        {
            ReadItems(gameInstance, 0x007531E8, gameInstance.Items);
        }
    }
}