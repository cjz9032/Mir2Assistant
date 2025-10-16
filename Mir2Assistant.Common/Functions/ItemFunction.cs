using System.Diagnostics;
using Mir2Assistant.Common.Constants;
using Mir2Assistant.Common.Models;
using Serilog;
// (Stdmode)
// 0 药品 （MAC2=时间,Dc=+攻击,Sc=+道术,Mc=+魔法,Ac2=+速度,Ac=+HP,Mac=+MP） 
// 1 食物  
// 2 特殊 传送石、聚灵珠、祝福罐、书页包、修复神水、死亡记录卷等 
// 3 卷类  
// 4 技能书  
// 5 武器 物品Anicount字段设置数字188，将代表武器带有 倚天技能 
// 6 武器 （Ac2=准,Mac=诅咒,Mac2=攻击速度-,Source=神圣,Ac=幸运） 
// 7 补充 气血石、幻魔石、魔血石、千里传音等 
// 10 衣服(男) 物品Anicount字段设置数字0-27，将代表不同外观的翅膀 
// 11 衣服(女) 物品Anicount字段设置数字0-27，将代表不同外观的翅膀 
// 15 头盔  
// 16 斗笠、面巾 物品Anicount字段设置数字4、6、7，将代表不同外观的斗笠 
// 19 项链 （Ac2=魔法躲避,Mac=诅咒,Mac2=幸运） 
// 20 项链 （Ac2=准,Mac2=敏） 
// 21 项链 （Ac=速度+,Ac2=HP恢复,Mac=速度-,Mac2=MP 恢复）  
// 22 戒指   
// 23 戒指 （Ac=速度+,Ac2=毒物躲避,Mac=速度-,Mac2=中毒恢复） 
// 24 手镯 （Ac2=准确,Mac2=敏捷） 
// 25 符、毒药  
// 26 手镯   
// 27 腰带   
// 28 鞋子 物品Anicount字段设置数字，将代表可以增加负重 
// 29 宝石   
// 30 照明物 物品数据内Source字段为1时不随时间掉持久，为0随时间掉持久 
// 31 捆装物品  
// 40 肉类 鸡肉、肉 
// 41 任务  
// 41 佣兵令牌 物品Shape=35,Looks=1132。 
// 42 制作原料 该类物品可补充火龙之心持久 
// 43 矿石   
// 44 行会相关 沃玛号角、祖玛头像、勋章之心 
// 45 特殊 骰子、筹码 
// 46 宝箱、钥匙 物品Shape字段设置对应数字，将代表可以开启对应的宝箱。 宝箱设置 
// 47 金米   
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
                        // var changed = false; todo 优化
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
                        item.Duration = memoryUtils.ReadToShort(itemAddr + GameState.MirConfig["物品持久"]) / 100;
                        item.MaxDuration = memoryUtils.ReadToShort(itemAddr + GameState.MirConfig["物品最大持久"]) / 100;
                        item.stdMode = memoryUtils.ReadToInt8(itemAddr + GameState.MirConfig["物品MODE"]);
                        item.addr = itemAddr;
                        item.reqType = memoryUtils.ReadToInt8(itemAddr + GameState.MirConfig["物品ReqType"]);
                        item.reqPoints = memoryUtils.ReadToInt8(itemAddr + GameState.MirConfig["物品ReqPts"]);

                        // 这里为了获取明确的业务意义的值,防御..
                        if (GameState.MirConfig["物品Ac"] > 0
                         && (EquipPosition)item.stdModeToUseItemIndex[0] != EquipPosition.Weapon
                         && (EquipPosition)item.stdModeToUseItemIndex[0] != EquipPosition.Necklace)
                        {
                            item.MinDef = memoryUtils.ReadToInt8(itemAddr + GameState.MirConfig["物品Ac"]);
                            item.MaxDef = memoryUtils.ReadToInt8(itemAddr + GameState.MirConfig["物品Ac2"]);
                            item.MinMageDef = memoryUtils.ReadToInt8(itemAddr + GameState.MirConfig["物品Mac"]);
                            item.MaxMageDef = memoryUtils.ReadToInt8(itemAddr + GameState.MirConfig["物品Mac2"]);
                        }
                        // JP determined manually
                        if (GameState.MirConfig["物品Ac"] > 0)
                        {
                            //  1. 对比DB
                            var 物品AC = GameState.MirConfig["物品Ac"];
                            for (int j = 0; j < 10; j++)
                            {
                                item.OriginCriticals[j] = memoryUtils.ReadToInt8(itemAddr + 物品AC + j);
                            }
                            var bs = GameConstants.Items.GetItemStats(item.Name);
                            if (bs != null)
                            {
                                // compare
                                var sumPts = 0;
                                for (int j = 0; j < 10; j++)
                                {
                                    sumPts += item.OriginCriticals[j] - bs[j];

                                }
                                if (sumPts > 0)
                                {
                                    item.GodPts = sumPts;
                                    item.IsGodly = true;
                                }
                            }
                            // force update by sp rule 1
                            // 项链幸运可以覆盖上面的JP判断
                            if (item.stdMode == 19)
                            {
                                // todo 重写IsGodly, 先这直接判断
                                // 19 项链 （Ac2=魔法躲避,Mac=诅咒,Mac2=幸运） 
                                item.MacEvasion = memoryUtils.ReadToInt8(itemAddr + GameState.MirConfig["物品Ac2"]);
                                item.Luck = memoryUtils.ReadToInt8(itemAddr + GameState.MirConfig["物品Mac2"]);
                                if (item.Luck > 0 || item.MacEvasion > 2)// todo 暂时测试用, 验证有效就+1 1和3
                                {
                                    item.GodPts = 99; // 为了突出保留低级别装备
                                    item.IsGodly = true;
                                }
                            }
                            // force update by sp rule 2
                            // 同样可以覆盖上面判断 设置100 by jpnamehashset

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
                byte nameLength = memoryUtils.ReadToInt8(itemAddr + GameState.MirConfig["地物NAME偏移"]);
                item.Name = memoryUtils.ReadToString(itemAddr + GameState.MirConfig["地物NAME偏移"] + 1, nameLength);
                if (GameState.MirConfig["地物极品点"] > 0)
                {
                    item.GodPts = memoryUtils.ReadToInt8(itemAddr + GameState.MirConfig["地物极品点"]);
                }
                else
                {
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

            ReadItems(gameInstance, (int)GameState.MirConfig["背包基址"] + ((int)GameState.MirConfig["物品SIZE"] * 6), gameInstance.Items);
            ReadItems(gameInstance, (int)GameState.MirConfig["背包基址"], gameInstance.QuickItems);
            // sw.Stop();
            // Log.Debug($"读取背包耗时: {sw.ElapsedMilliseconds}ms, 物品数量: {gameInstance.Items.Count}");
        }



        public static void Pickup(MirGameInstanceModel gameInstance)
        {
            SendMirCall.Send(gameInstance, 3031, new nint[] { });
        }
    }
}