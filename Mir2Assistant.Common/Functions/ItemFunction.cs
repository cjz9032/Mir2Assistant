using Mir2Assistant.Common.Models;

namespace Mir2Assistant.Common.Functions
{
    public static class ItemFunction
    {
        public static void ReadBag(MirGameInstanceModel gameInstance)
        {
            Task.Run(() =>
            {
                if (gameInstance.IsReadingItems)
                {
                    return;
                }
                gameInstance.IsReadingItems = true;
                var memoryUtils = gameInstance!.MemoryUtils!;
                var bagBaseAddr = 0x007531E8; // 背包基址
                int itemCount = 40; // 40个格子
                int itemSize = 0x80; // 每个item占80字节

                // 确保有40个物品项
                while (gameInstance.Items.Count < itemCount)
                {
                    int index = gameInstance.Items.Count;
                    gameInstance.Items[index] = new ItemModel { Index = index };
                }
                if (gameInstance.Items.Count > itemCount)
                {
                    for (int i = gameInstance.Items.Count - 1; i >= itemCount; i--)
                    {
                        gameInstance.Items.Remove(i);
                    }
                }

                for (int i = 0; i < itemCount; i++)
                {
                    var item = gameInstance.Items[i];
                    var itemAddr = bagBaseAddr + i * itemSize;
                    byte nameLength = memoryUtils.ReadToInt8(itemAddr);
                    item.IsEmpty = nameLength == 0;
                    
                    if (!item.IsEmpty)
                    {
                        item.Id = memoryUtils.ReadToInt(itemAddr + 0x74); // id偏移74的Int32
                        if (nameLength > 0)
                        {
                            item.Name = memoryUtils.ReadToString(itemAddr + 1, nameLength);
                        }
                        else
                        {
                            item.Name = string.Empty;
                        }
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
    }
}