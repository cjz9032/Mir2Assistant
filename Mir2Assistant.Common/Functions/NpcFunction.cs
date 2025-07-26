using Mir2Assistant.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mir2Assistant;
using Mir2Assistant.Common.Utils;
using Serilog; // Add this reference to access DllInject

namespace Mir2Assistant.Common.Functions
{
    public static class NpcFunction
    {

        /// <summary>
        /// 提取对话命令
        /// </summary>
        /// <param name="dialog"></param>
        /// <returns></returns>
        public static List<string> GetTalkCmds(string dialog)
        {
            var ret = new List<string>();
            if (string.IsNullOrEmpty(dialog))
            {
                return ret;
            }
            var regex = new Regex(@"\<(.*?)\/(.*?)\>");
            Match match = regex.Match(dialog);
            while (match.Success)
            {
                if (match.Value.Contains("/@"))
                {
                    ret.Add(match.Value.TrimStart('<').TrimEnd('>'));
                }
                match = match.NextMatch();
            }
            return ret;
        }

        private static async Task<string> Talk(MirGameInstanceModel gameInstance, Action act)
        {
            gameInstance.TalkCmds.Clear();
            act();

            var memoryUtil = gameInstance!.MemoryUtils!;
            var base2 = memoryUtil.GetMemoryAddress(gameInstance.MirConfig["对话框基址"], 0);
            var prevAddr2 = memoryUtil.GetMemoryAddress(base2) + 0xC40;
            var prevAddr = memoryUtil.GetMemoryAddress(prevAddr2, 0);
            nint resAddr = 0;
            if (!await TaskWrapper.Wait(() =>
            {
                var nowAddr = memoryUtil.GetMemoryAddress(prevAddr2, 0);
                resAddr = nowAddr;
                return prevAddr != nowAddr && nowAddr != 0;
            }))
            {
                //TODO 可能会出错, 先用着, 直接读了旧地址 认为不存在, 额外可以判断一些信息得到, 比如字符串是否合法等
                var nowAddr = memoryUtil.GetMemoryAddress(prevAddr2, 0);
                if (nowAddr != 0)
                {
                    var str222 = memoryUtil.ReadToDelphiUnicode(nowAddr);
                    gameInstance.TalkCmds = GetTalkCmds(str222);
                    return str222;
                }
            }

            var str = memoryUtil.ReadToDelphiUnicode(resAddr);
            gameInstance.TalkCmds = GetTalkCmds(str);
            return str;
        }


        /// <summary>
        /// 点NPC
        /// </summary>
        /// <param name="gameInstance"></param>
        /// <param name="NPC"></param>
        /// <returns></returns>
        public static async Task<string> ClickNPC(MirGameInstanceModel gameInstance, MonsterModel NPC)
        {
            return await Talk(gameInstance, () =>
            {
                SendMirCall.Send(gameInstance, 3001, [NPC.Id]);
            });
        }

        public static async Task<string> ClickNPC(MirGameInstanceModel gameInstance, string NpcName)
        {
            MonsterModel? npc = null;
            if (!await TaskWrapper.Wait(() =>
            {
                npc = gameInstance.Monsters.Values.FirstOrDefault(o => o.TypeStr == "NPC" && o.Name == NpcName);
                return npc != null;
            }))
            {
                return "";
            }
            return await ClickNPC(gameInstance, npc!);
        }

        /// <summary>
        /// 二级对话
        /// </summary>
        /// <param name="gameInstance"></param>
        /// <param name="cmd"></param>
        public static async Task<string> Talk2(MirGameInstanceModel gameInstance, string cmd)
        {
            return await Talk(gameInstance, () =>
            {
                nint[] data = MemoryUtils.PackStringsToData(cmd);
                SendMirCall.Send(gameInstance, 3002, data);
            });
        }

        // 文字没用
        // public static async Task<string> Talk2Text(MirGameInstanceModel gameInstance, string text)
        // {
        //     string? cmd = null;
        //     if (!await TaskWrapper.Wait(() =>
        //     {
        //         cmd = gameInstance.TalkCmds.FirstOrDefault(o => o.Split("/")[0] == text);
        //         return cmd != null;
        //     }))
        //     {
        //         return "";
        //     }
        //     return await Talk2(gameInstance, cmd!.Split("/")[1]);
        // }

        /// <summary>
        /// 等NPC出现
        /// </summary>
        /// <param name="gameInstance"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static async Task<bool> WaitNPC(MirGameInstanceModel gameInstance, string npcName, int timeout = 50)
        {
            return await TaskWrapper.Wait(() => gameInstance.Monsters.Values.Any(o => o.TypeStr == "NPC" && o.Name == npcName), timeout);
        }

        /// <summary>
        /// 盲选蜡烛
        /// </summary>
        /// <param name="gameInstance"></param>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public async static Task BuyLZ(MirGameInstanceModel gameInstance, string itemName, int count = 2)
        {

            nint[] data = MemoryUtils.PackStringsToData(itemName);
            await Talk2(gameInstance!, "@buy");
            await Task.Delay(500);
            SendMirCall.Send(gameInstance, 3005, data);
            await Task.Delay(800);
            // 盲选
            for (int i = 0; i < count; i++)
            {
                var memoryUtils = gameInstance!.MemoryUtils!;
                var addr = memoryUtils.GetMemoryAddress(0x74350C, 0xC6C);
                memoryUtils.WriteInt(addr, i);
                await Task.Delay(300);
                SendMirCall.Send(gameInstance, 3006, new nint[] { i });
                await Task.Delay(300);
            }

        }

        /// <summary>
        /// 购买物品
        /// </summary>
        /// <param name="gameInstance"></param>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public async static Task BuyDrug(MirGameInstanceModel gameInstance, string itemName, int count = 1)
        {
            nint[] data = MemoryUtils.PackStringsToData(itemName);
            for (int i = 0; i < count; i++)
            {
                SendMirCall.Send(gameInstance, 3010, data);
                await Task.Delay(300);
            }
        }

        /// <summary>
        /// 吃东西
        /// </summary>
        /// <param name="gameInstance"></param>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public static void EatIndexItem(MirGameInstanceModel gameInstance, string itemName)
        {
            ItemFunction.ReadBag(gameInstance);
            var bagItems2 = gameInstance.Items;
            var idx = bagItems2.FindIndex(o => o.Name == itemName);
            if (idx >= 0)
            {
                SendMirCall.Send(gameInstance, 3019, new nint[] { idx + 6 });
            }
            else
            {
                var quickItems = gameInstance.QuickItems;
                var idx2 = quickItems.FindIndex(o => o.Name == itemName);
                if (idx2 >= 0)
                {
                    SendMirCall.Send(gameInstance, 3019, new nint[] { idx2 });
                }
            }
        }

        /// <summary>
        /// 脱东西
        /// </summary>
        /// <param name="gameInstance"></param>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public async static Task<ItemModel?> TakeOffItem(MirGameInstanceModel gameInstance, EquipPosition pos)
        {
            var item = gameInstance.CharacterStatus.useItems[(int)pos];
            if (!item.IsEmpty)
            {
                var itemCopy = new ItemModel
                {
                    Id = item.Id,
                    Index = item.Index,
                    Name = item.Name,
                    Duration = item.Duration,
                    MaxDuration = item.MaxDuration,
                    stdMode = item.stdMode,
                    IsGodly = item.IsGodly
                };

                var data = Common.Utils.StringUtils.GenerateMixedData(
                    item.Name,
                    item.Index,
                    item.Id
                );
                SendMirCall.Send(gameInstance!, 3022, data);
                gameInstance!.MemoryUtils!.WriteByte(item.addr, 0);
                await Task.Delay(500);
                return itemCopy;
            }
            return null;
        }

        /// <summary>
        /// 修东西
        /// </summary>
        /// <param name="gameInstance"></param>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public async static Task RepairItem(MirGameInstanceModel gameInstance, ItemModel item)
        {
            nint[] data = Mir2Assistant.Common.Utils.StringUtils.GenerateCompactStringData(item.Name);
            Array.Resize(ref data, data.Length + 1);
            data[data.Length - 1] = item.Id;

            SendMirCall.Send(gameInstance!, 3012, data);
            await Task.Delay(500);
        }

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="gameInstance"></param>
        /// <returns></returns>
        public async static Task RefreshPackages(MirGameInstanceModel gameInstance)
        {
            SendMirCall.Send(gameInstance!, 9010, new nint[] { });
            await Task.Delay(500);
            ItemFunction.ReadBag(gameInstance);
        }

        public async static Task<bool> CheckNeedRep(MirGameInstanceModel gameInstance, EquipPosition position)
        {
            var item = gameInstance.CharacterStatus.useItems[(int)position];
            if (item.IsEmpty)
            {
                return false;
            }
            if (item.Duration < 2000 || (item.Duration / item.MaxDuration) < 0.8)
            {
                return false;
            }
            return true;
        }

        public async static Task<bool> CheckNeedBuy(MirGameInstanceModel gameInstance, EquipPosition position)
        {
            var item = gameInstance.CharacterStatus.useItems[(int)position];
            if (item.IsEmpty)
            {
                return true;
            }
            return false;
        }

        public async static Task<bool> CheckExistsInBags(MirGameInstanceModel gameInstance, string name)
        {
            var item = gameInstance.Items.Where(o => o.Name == name).FirstOrDefault();

            return item != null && !item.IsEmpty;
        }
        /// <summary>
        /// 修理指定位置的装备
        /// </summary>
        /// <param name="gameInstance">游戏实例</param>
        /// <param name="npcName">NPC名称</param>
        /// <param name="position">装备位置</param>
        /// <param name="x">NPC的X坐标</param>
        /// <param name="y">NPC的Y坐标</param>
        /// <returns></returns>
        public async static Task RepairEquipment(MirGameInstanceModel gameInstance, string npcName, EquipPosition position, int x, int y)
        {
            var needRep = await CheckNeedRep(gameInstance, position);
            if (!needRep)
            {
                return;
            }

            Log.Information($"修理{npcName}的{position}装备");
            bool pathFound = await GoRunFunction.PerformPathfinding(CancellationToken.None, gameInstance!, x, y, "", 6);
            if (pathFound)
            {
                await ClickNPC(gameInstance!, npcName);
                await Talk2(gameInstance!, "@repair");

                await Task.Delay(500);
                var taked = await TakeOffItem(gameInstance, position);
                if (taked != null)
                {
                    await RepairItem(gameInstance, taked);
                    await Task.Delay(1000);
                    await RefreshPackages(gameInstance);
                }
            }
        }

        public async static Task BuyEquipment(MirGameInstanceModel gameInstance, string npcName, EquipPosition position, int x, int y)
        {
            var need = await CheckNeedBuy(gameInstance, position);
            if (!need)
            {
                return;
            }

            Log.Information($"购买{npcName}的{position}装备");
            bool pathFound = await GoRunFunction.PerformPathfinding(CancellationToken.None, gameInstance!, x, y, "", 6);
            if (pathFound)
            {
                await ClickNPC(gameInstance!, npcName);
                await Talk2(gameInstance!, "@buy");

                var itemNames = new List<string>();
                var genderStr = gameInstance.AccountInfo.Gender == 1 ? "(男)" : "(女)";
                var CharacterStatus = gameInstance.CharacterStatus;
                // todo 自动推荐装备, 目前写死
                switch (position)
                {
                    // todo 性别
                    case EquipPosition.Dress:
                        itemNames.Add("布衣" + genderStr);
                        if (gameInstance.AccountInfo.role != RoleType.mage)
                        {
                            if (CharacterStatus.Level >= 11)
                            {
                                itemNames.Add("轻型盔甲");
                            }
                        }
                        else
                        {
                            if (CharacterStatus.Level >= 11)
                            {
                                itemNames.Add("轻型盔甲");
                            }
                        }
                        break;
                    case EquipPosition.Weapon:
                        itemNames.Add("木剑");
                        if (gameInstance.AccountInfo.role != RoleType.mage)
                        {
                            if (CharacterStatus.Level >= 5)
                            {
                                itemNames.Add("青铜剑");
                            }
                            if (CharacterStatus.Level >= 10)
                            {
                                itemNames.Add("铁剑");
                            }
                            if (CharacterStatus.Level >= 13)
                            {
                                itemNames.Add("青铜斧");
                            }
                        }
                        else
                        {
                            if (CharacterStatus.Level >= 15)
                            {
                                itemNames.Add("海魂");
                            }
                        }
                        break;
                }


                // test 
                //itemNames = new List<string>{"木剑" , "青铜剑" ,"青铜斧", "降魔"};


                if (itemNames.Count == 0)
                {
                    return;
                }

                var memoryUtils = gameInstance.MemoryUtils!;

              

                itemNames.Reverse();
                var menuListLen = 0;
                // 从高到低找 , 找不到就用前面的
                for (int i = 0; i < itemNames.Count; i++)
                {

                    // 已经检测过存在了, 只看是否为空先
                    var name = itemNames[i];
                    var exists = await CheckExistsInBags(gameInstance, name);
                    if (exists)
                    {
                        continue;
                    }
                    nint[] data = MemoryUtils.PackStringsToData(name);
                    SendMirCall.Send(gameInstance, 3005, data);
                    await Task.Delay(800);
                    // 判断是否存在
                    // len [[0x74350C]+0x00000C5C]+08
                    menuListLen = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(memoryUtils.GetMemoryAddress(0x74350C, 0x00000C5C, 08)));
                    if (menuListLen > 0)
                    {
                        break;
                    }
                    // 盲选
                }
                if (menuListLen == 0)
                {
                    return;
                }
                var addr = memoryUtils.GetMemoryAddress(0x74350C, 0xC6C);
                memoryUtils.WriteInt(addr, 0);
                await Task.Delay(300);
                SendMirCall.Send(gameInstance, 3006, new nint[] { 0 });
                await Task.Delay(500);
            }
        }

        public async static Task autoReplaceEquipment(MirGameInstanceModel instance)
        {
            var CharacterStatus = instance.CharacterStatus;
            var bagItems = instance.Items;
            foreach (var itemWithIndex in CharacterStatus.useItems.Select((item, index) => new { item, index }))
            {
                var item = itemWithIndex.item;
                var index = itemWithIndex.index;
                // TODO 装备评分
                // 先非常简略从背包找乌木剑, 并且手上是木剑, 后面再优化
                if (item.Name == "木剑")
                {
                    var final = bagItems.Where(o => o.Name == "乌木剑" && !o.IsLowDurability).FirstOrDefault();
                    if (final != null)
                    {
                        nint toIndex = index;
                        nint bagGridIndex = final.Index;
                        SendMirCall.Send(instance, 3021, new nint[] { bagGridIndex, toIndex });
                        await Task.Delay(800);
                        ItemFunction.ReadBag(instance);
                    }
                }
                if (item.IsEmpty)
                {
                    // 从bags里找装备, 要符合条件
                    // 1.index->stdmode
                    // 2.not IsLowDurability
                    // 3.能携带, 目前只看等级, reqType  // todo 更多类型,以及携带策略可能要搭配
                    // 4.TODO 负重腕力等, 先不管
                    // 5. 排序等级第一名, 剩余持久度第一名
                    var final = bagItems.Where(o => o.stdModeToUseItemIndex.Contains((byte)index)
                    // && !o.IsLowDurability
                    && o.reqType == 0
                    && o.reqPoints <= CharacterStatus.Level
                    ).OrderByDescending(o => o.reqPoints).ThenByDescending(o => o.Duration).FirstOrDefault();
                    if (final != null)
                    {
                        // 装回检查的位置
                        nint toIndex = index;
                        nint bagGridIndex = final.Index;
                        SendMirCall.Send(instance, 3021, new nint[] { bagGridIndex, toIndex });
                        await Task.Delay(800);
                        ItemFunction.ReadBag(instance);
                    }
                }
                else
                {
                    // TODO 如果发现比身上更NB的,需要比较按职业
                    // 目前只比较req 0(等级) 和 reqp 
                    var final = bagItems.Where(o => o.stdModeToUseItemIndex.Contains((byte)index)
                    && o.reqType == 0
                    && o.reqPoints <= CharacterStatus.Level
                    && o.reqPoints > item.reqPoints
                    ).OrderByDescending(o => o.reqPoints).ThenByDescending(o => o.Duration).FirstOrDefault();

                    if (final != null)
                    {
                        // 装回检查的位置
                        nint toIndex = index;
                        nint bagGridIndex = final.Index;
                        SendMirCall.Send(instance, 3021, new nint[] { bagGridIndex, toIndex });
                        await Task.Delay(800);
                        ItemFunction.ReadBag(instance);

                    }

                }
            }
        }
    }
}
