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

            var memoryUtil = gameInstance!.memoryUtils!;
            var base2 = memoryUtil.GetMemoryAddress(GameState.MirConfig["对话框基址"], 0);
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
            gameInstance.GameDebug("尝试点击NPC: {Name}", NpcName);
            MonsterModel? npc = null;
            if (!await TaskWrapper.Wait(() =>
            {
                npc = gameInstance.Monsters.Values.FirstOrDefault(o => o.TypeStr == "NPC" && o.Name == NpcName);
                return npc != null;
            }))
            {
                gameInstance.GameWarning("未找到NPC: {Name}", NpcName);
                return "";
            }
            gameInstance.GameDebug("找到NPC: {Name}, 位置: ({X}, {Y})", NpcName, npc.X, npc.Y);
            return await ClickNPC(gameInstance, npc!);
        }

        /// <summary>
        /// 二级对话
        /// </summary>
        /// <param name="gameInstance"></param>
        /// <param name="cmd"></param>
        public static async Task<string> Talk2(MirGameInstanceModel gameInstance, string cmd)
        {
            gameInstance.GameDebug("执行NPC对话命令: {Command}", cmd);
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
        /// 盲选蜡烛
        /// </summary>
        /// <param name="gameInstance"></param>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public async static Task BuyLZ(MirGameInstanceModel gameInstance, string itemName, int count = 2)
        {
            gameInstance.GameInfo("购买物品: {Item}, 数量: {Count}", itemName, count);
            nint[] data = MemoryUtils.PackStringsToData(itemName);
            await Talk2(gameInstance!, "@buy");
            await Task.Delay(500);
            SendMirCall.Send(gameInstance, 3005, data);
            await Task.Delay(800);
            // 盲选
            for (int i = 0; i < count; i++)
            {
                gameInstance.GameDebug("执行第 {Index} 次购买", i + 1);
                var memoryUtils = gameInstance!.memoryUtils!;
                var addr = memoryUtils.GetMemoryAddress(0x74350C, 0xC6C);
                memoryUtils.WriteInt(addr, i);
                await Task.Delay(300);
                SendMirCall.Send(gameInstance, 3006, new nint[] { i });
                await Task.Delay(300);
            }

        }

        /// <summary>
        /// 购买药品
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
                await Task.Delay(800);
            }
        }

        /// <summary>
        /// 卖东西
        /// </summary>
        /// <param name="gameInstance"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public async static Task SellItems(MirGameInstanceModel gameInstance, List<ItemModel> items)
        {
            foreach (var item in items)
            {
                var data = Common.Utils.StringUtils.GenerateMixedData(
                    item.Name,
                    item.Id
                );

                SendMirCall.Send(gameInstance!, 3011, data);
                await Task.Delay(500);
            }

        }

        /// <summary>
        /// 吃东西
        /// </summary>
        /// <param name="gameInstance"></param>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public static void EatIndexItem(MirGameInstanceModel gameInstance, int idx, bool force = false)
        {
            if (!force && gameInstance.eatItemLastTime + 3000 > Environment.TickCount)
            {
                gameInstance.GameDebug("物品使用冷却中，跳过使用物品: {Index}", idx);
                return;
            }
            gameInstance.GameDebug("使用物品，索引: {Index}", idx);
            gameInstance.eatItemLastTime = Environment.TickCount;
            SendMirCall.Send(gameInstance, 3019, new nint[] { idx });
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
                gameInstance.GameDebug("准备脱下装备: {Name}, 位置: {Position}, 耐久: {Duration}/{MaxDuration}", 
                    item.Name, pos, item.Duration, item.MaxDuration);
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
                gameInstance!.memoryUtils!.WriteByte(item.addr, 0);
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
            await Task.Delay(1000);
            ItemFunction.ReadBag(gameInstance);
        }

        // todo 目前只能城市调用
        public static bool CheckNeedRep(MirGameInstanceModel gameInstance, ItemModel item)
        {
            if (item.IsEmpty)
            {
                // 空了就不用修了
                return false;
            }
            if (item.Duration < 20 || ((double)item.Duration / (double)item.MaxDuration) < 0.8)
            {
                return true;
            }
            return false;
        }

        public static List<string>? CheckPreferComparedUsed(MirGameInstanceModel gameInstance, EquipPosition position, bool careJPDurability = false)
        {
            // 要买比已有的装备推荐等级更高的,也就是N->MAX N>=0
            var preferItems = preferStdEquipment(gameInstance, position);
            var item = gameInstance.CharacterStatus.useItems[(int)position];
            // care则把低极品可换下来, 不care则不换
            if (item.IsEmpty || (careJPDurability ? item.IsGodly && item.IsLowDurability : false))
            {
                // full 
                return preferItems;
            }
            // 或者是比推荐的位置低, 这首先得保证推荐产品是全的, 不然会不准确而被替换
            if (preferItems.Count == 0 || item.IsGodly)
            {
                return null;
            }
            // 然后就是普通的比较
            var index = preferItems.FindIndex(o => o == item.Name);
            if (index == 0) return null; // 已经是顶级装备，不需要升级
            return index == -1 ? preferItems : preferItems.GetRange(0, index);
        }

        public async static Task<bool> CheckExistsInBags(MirGameInstanceModel gameInstance, string name)
        {
            var item = gameInstance.Items.Where(o => !o.IsEmpty && o.Name == name).FirstOrDefault();
            return item != null;
        }

        public static string PickNearHomeMap(MirGameInstanceModel gameInstance)
        {
            if(new string[] {"0","1","2","3"}.Contains(gameInstance.CharacterStatus.MapId))
            {
                return gameInstance.CharacterStatus.MapId;
            }
            // 根据当前所在地图, 找到最近的回家点
            var home = "0";
            var mapId = gameInstance.CharacterStatus.MapId;
            // 毒蛇山谷
            if (mapId.StartsWith("D42") || mapId.StartsWith("E6"))
            {
                home = "2";
            }
            // 土城
            // 蜈蚣
            if (mapId.StartsWith("D6") || mapId.StartsWith("E7") || mapId.StartsWith("D5"))
            {
                home = "3";
            }
            return home;
        }

        public static (string map, string npcName, int x, int y) PickDrugNpcByMap(MirGameInstanceModel gameInstance, string mapId)
        {
            // 根据当前所在地图, 找到最近的NPC
            if (mapId == "0")
            {
                return ("0", "边界村小店老板", 290, 611);
            }
            else
            {
                // 其他大图
                return ("0", "", 0, 0);
            }
        }
        public static (string map, string npcName, int x, int y) PickEquipNpcByMap(MirGameInstanceModel gameInstance, EquipPosition position, string mapId)
        {
            // 根据当前所在地图, 找到最近的NPC
            if (mapId == "0")
            {
                // 固定为左下角 因为只有这全有买卖, 除了蜡烛
                switch (position)
                {
                    case EquipPosition.Weapon:
                        return ("0", "边界村铁匠铺", 295, 608);
                    case EquipPosition.Dress:
                    case EquipPosition.Helmet:
                        return ("0", "白家服装老板", 304, 608);
                    case EquipPosition.Necklace:
                        return ("0141", "项链店老板", 9, 10);
                    case EquipPosition.ArmRingLeft:
                    case EquipPosition.ArmRingRight:
                        return ("0141", "手镯店老板", 16, 16);
                    case EquipPosition.RingLeft:
                    case EquipPosition.RingRight:
                        return ("0141", "戒指店老板", 23, 23);
                    default:
                        return ("0", "", 0, 0);
                }
            }
            else
            {
                // 其他大图
                return ("0", "", 0, 0);
            }
            // return PickNpcByMap(gameInstance, position, mapId);
        }


        public async static Task RepairAllEquipment(MirGameInstanceModel gameInstance, CancellationToken _cancellationToken)
        {
            var nearHome = PickNearHomeMap(gameInstance);  
            foreach (var position in Enum.GetValues(typeof(EquipPosition)))
            {

                var (npcMap, npcName, x, y) = PickEquipNpcByMap(gameInstance, (EquipPosition)position, nearHome);
                var needRep = CheckNeedRep(gameInstance, gameInstance.CharacterStatus.useItems[(int)position]);
                if (!needRep)
                {
                    continue;
                }

                gameInstance.GameInfo($"修理{npcName}的{position}装备");
                bool pathFound = await GoRunFunction.PerformPathfinding(CancellationToken.None, gameInstance!, x, y, npcMap, 6);
                if (pathFound)
                {
                    await ClickNPC(gameInstance!, npcName);
                    await Talk2(gameInstance!, "@repair");
                    await Task.Delay(500);
                    var taked = await TakeOffItem(gameInstance, (EquipPosition)position);
                    if (taked != null)
                    {
                        await RepairItem(gameInstance, taked);
                        await Task.Delay(1000);
                        await RefreshPackages(gameInstance);
                    }
                    // trigger takeon 
                    await autoReplaceEquipment(gameInstance, false);
                }
            }
        }
        
        public async static Task RepairAllBagsEquipment(MirGameInstanceModel gameInstance, CancellationToken _cancellationToken)
        {
            var nearHome = PickNearHomeMap(gameInstance);  
            foreach (var position in new EquipPosition[] { EquipPosition.Weapon, EquipPosition.Dress })
            {
                var needRep = CheckNeedRep(gameInstance, gameInstance.CharacterStatus.useItems[(int)position]);
                if (!needRep)
                {
                    continue;
                }

                // 找背包内的对应的东西, 目前是1 , 保留多个的能力
                var items = gameInstance.Items.Where(o => !o.IsEmpty && o.stdModeToUseItemIndex.Length > 0
                // todo 这里如果是首饰还需要继续优化[0], 目前只修武器和衣服
                 && o.stdModeToUseItemIndex[0] == (byte)position).ToList();
                if(items.Count == 0)
                {
                    continue;
                }
                gameInstance.GameInfo($"背包内保留的{position}装备: {items.Count}个");
                // 找到对应的NPC
                var (npcMap, npcName, x, y) = PickEquipNpcByMap(gameInstance, (EquipPosition)position, nearHome);
                gameInstance.GameInfo($"修理背包内保留的{npcName}的{position}装备");
                bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationToken, gameInstance!, x, y, npcMap, 6);
                if (pathFound)
                {
                    await ClickNPC(gameInstance!, npcName);
                    await Talk2(gameInstance!, "@repair");
                    await Task.Delay(500);
                    foreach (var item in items)
                    {
                        await RepairItem(gameInstance, item);
                        await Task.Delay(1000);
                    }
                    await RefreshPackages(gameInstance);
                }
            }
        }

        public async static Task sellLJEquipment(MirGameInstanceModel gameInstance, CancellationToken _cancellationToken)
        {
            var nearHome = PickNearHomeMap(gameInstance);
            // 找到所有的装备除了极品 NPC分组去卖了, 
            var ljequipment = gameInstance.Items.Where(o => !o.IsEmpty && !o.IsGodly && o.stdModeToUseItemIndex.Length > 0)
            .GroupBy(o => o.stdModeToUseItemIndex[0]); // 可以只0, 因为是同一个NPC
            foreach (var group in ljequipment)
            {
                var position = (EquipPosition)group.Key;
                var lists = group.ToList();
                // 可以保留武器和衣服, 因为最容易破, 其他都卖了
                if (position == EquipPosition.Weapon || position == EquipPosition.Dress)
                {
                    // 找到推荐的
                    var preferItems = preferStdEquipment(gameInstance, position);
                    if (preferItems != null && preferItems.Count > 0)
                    {
                        // 只保留一个最好的，优先极品，然后按推荐顺序
                        ItemModel? keepItem = null;
                        foreach (var preferName in preferItems)
                        {
                            keepItem = lists.Where(o => o.Name == preferName).FirstOrDefault();
                            if (keepItem != null) break;
                        }

                        // 如果找到了要保留的装备，从卖出列表中移除
                        if (keepItem != null)
                        {
                            lists.Remove(keepItem);
                            gameInstance.GameInfo($"保留备用{position}: {keepItem.Name} (IsGodly: {keepItem.IsGodly})");
                        }
                    }
                }
                // 保留完可能就空了 跳过该轮
                if (lists.Count == 0)
                {
                    continue;
                }
                var (npcMap, npcName, x, y) = PickEquipNpcByMap(gameInstance, position, nearHome);

                gameInstance.GameInfo($"出售{npcName}的{position}装备");
                bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationToken, gameInstance!, x, y, npcMap, 6);
                if (pathFound)
                {
                    await ClickNPC(gameInstance!, npcName);
                    await Talk2(gameInstance!, "@sell");
                    await Task.Delay(500);
                    await SellItems(gameInstance, lists);
                }
            }
            await RefreshPackages(gameInstance);
        }

        public static List<string> preferStdEquipment(MirGameInstanceModel gameInstance, EquipPosition position)
        {
            var itemNames = new List<string>();
            var genderStr = gameInstance.AccountInfo.Gender == 1 ? "(男)" : "(女)";
            var CharacterStatus = gameInstance.CharacterStatus;
            // 自动推荐装备
            switch (position)
            {
                // todo 性别
                case EquipPosition.Necklace:
                    if (CharacterStatus.Level >= 3)
                    {
                        itemNames.Add("金项链");
                    }
                    if (CharacterStatus.Level >= 3)
                    {
                        itemNames.Add("传统项链");
                    }
                    if (CharacterStatus.Level >= 17)
                    {
                        if (gameInstance.AccountInfo.role == RoleType.blade)
                        {
                            itemNames.Add("魔鬼项链");
                        }
                        else if (gameInstance.AccountInfo.role == RoleType.taoist)
                        {
                            itemNames.Add("凤凰明珠");
                        }
                    }
                    break;
                case EquipPosition.ArmRingLeft:
                case EquipPosition.ArmRingRight:
                    if (CharacterStatus.Level >= 3)
                    {
                        itemNames.Add("铁手镯");
                    }
                    if (CharacterStatus.Level >= 7)
                    {
                        itemNames.Add("皮质手套");
                    }
                    if (CharacterStatus.Level >= 8)
                    {
                        itemNames.Add("钢手镯");
                    }
                    if (CharacterStatus.Level >= 9)
                    {
                        itemNames.Add("大手镯");
                    }
                    if (gameInstance.AccountInfo.role == RoleType.blade)
                    {
                        if (CharacterStatus.Level >= 18)
                        {
                            itemNames.Add("坚固手套");
                        }
                    }
                    break;
                case EquipPosition.RingLeft:
                case EquipPosition.RingRight:
                    if (CharacterStatus.Level >= 3)
                    {
                        itemNames.Add("古铜戒指");
                    }
                    if (CharacterStatus.Level >= 9)
                    {
                        itemNames.Add("牛角戒指");
                    }
                    if (gameInstance.AccountInfo.role != RoleType.mage)
                    {
                        if (CharacterStatus.Level >= 16)
                        {
                            itemNames.Add("蓝色水晶戒指");
                        }
                    }
                    break;
                case EquipPosition.Helmet:
                    if (CharacterStatus.Level >= 10)
                    {
                        itemNames.Add("青铜头盔");
                    }
                    if (CharacterStatus.Level >= 14)
                    {
                        itemNames.Add("魔法头盔");
                    }
                    if (CharacterStatus.Level >= 23)
                    {
                        itemNames.Add("道士头盔");
                    }
                    break;
                case EquipPosition.Dress:
                    itemNames.Add("布衣" + genderStr);
                    if (gameInstance.AccountInfo.role != RoleType.mage)
                    {
                        if (CharacterStatus.Level >= 11)
                        {
                            itemNames.Add("轻型盔甲" + genderStr);
                        }
                        if (CharacterStatus.Level >= 16)
                        {
                            itemNames.Add("中型盔甲" + genderStr);
                        }
                        if (gameInstance.AccountInfo.role == RoleType.blade)
                        {
                            if (CharacterStatus.Level >= 22)
                            {
                                itemNames.Add("重盔甲" + genderStr);
                            }
                        }
                        else
                        {
                            if (CharacterStatus.Level >= 22)
                            {
                                itemNames.Add("灵魂战衣" + genderStr);
                            }
                        }

                    }
                    else
                    {
                        if (CharacterStatus.Level >= 11)
                        {
                            itemNames.Add("轻型盔甲" + genderStr);
                        }
                        if (CharacterStatus.Level >= 20)
                        {
                            itemNames.Add("中型盔甲" + genderStr);
                        }
                        if (CharacterStatus.Level >= 22)
                        {
                            itemNames.Add("魔法长袍" + genderStr);
                        }
                    }
                    break;
                case EquipPosition.Weapon:
                    itemNames.Add("木剑");
                    if (gameInstance.AccountInfo.role != RoleType.mage)
                    {
                        if (CharacterStatus.Level >= 2)
                        {
                            itemNames.Add("匕首");
                        }
                        if (CharacterStatus.Level >= 5)
                        {
                            itemNames.Add("青铜剑");
                        }
                        if (CharacterStatus.Level >= 10)
                        {
                            itemNames.Add("铁剑");
                            itemNames.Add("短剑");
                        }
                        if (CharacterStatus.Level >= 13)
                        {
                            itemNames.Add("青铜斧");
                        }
                        if (CharacterStatus.Level >= 15)
                        {
                            if (gameInstance.AccountInfo.role == RoleType.blade)
                            {
                                itemNames.Add("八荒");
                            }
                            else
                            {
                                itemNames.Add("半月");
                            }
                        }
                        if (gameInstance.AccountInfo.role == RoleType.blade)
                        {
                            if (CharacterStatus.Level >= 19)
                            {
                                itemNames.Add("凌风");
                            }
                            if (CharacterStatus.Level >= 20)
                            {
                                itemNames.Add("破魂");
                            }
                            if (CharacterStatus.Level >= 22)
                            {
                                itemNames.Add("修罗");
                            }
                        }
                        if (gameInstance.AccountInfo.role == RoleType.taoist)
                        {
                            if (CharacterStatus.Level >= 20)
                            {
                                itemNames.Add("降魔");
                            }
                        }
                    }
                    else
                    {
                        if (CharacterStatus.Level >= 5)
                        {
                            itemNames.Add("青铜剑");
                        }
                        if (CharacterStatus.Level >= 15)
                        {
                            itemNames.Add("海魂");
                        }
                    }
                    break;
            }
            itemNames.Reverse();
            return itemNames;
        }

        public async static Task buyAllEquipment(MirGameInstanceModel gameInstance, CancellationToken _cancellationToken)
        {
            foreach (var position in Enum.GetValues(typeof(EquipPosition)))
            {
                var preferBuyItems = CheckPreferComparedUsed(gameInstance, (EquipPosition)position);
                if (preferBuyItems == null)
                {
                    continue;
                }
                var nearHome = PickNearHomeMap(gameInstance);  
                var (npcMap, npcName, x, y) = PickEquipNpcByMap(gameInstance, (EquipPosition)position, nearHome);
                gameInstance.GameInfo($"购买{position}装备");
                bool pathFound = await GoRunFunction.PerformPathfinding(CancellationToken.None, gameInstance!, x, y, npcMap, 6);
                if (pathFound)
                {
                    await ClickNPC(gameInstance!, npcName);
                    await Talk2(gameInstance!, "@buy");

                    var memoryUtils = gameInstance.memoryUtils!;
                    var menuListLen = 0;
                    // 从高到低找 , 找不到就用前面的
                    for (int i = 0; i < preferBuyItems.Count; i++)
                    {

                        // 已经检测过存在了, 只看是否为空先
                        var name = preferBuyItems[i];
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
                        continue;
                    }
                    var addr = memoryUtils.GetMemoryAddress(0x74350C, 0xC6C);
                    memoryUtils.WriteInt(addr, 0);
                    await Task.Delay(300);
                    SendMirCall.Send(gameInstance, 3006, new nint[] { 0 });
                    await Task.Delay(500);

                    // trigger takeon 
                    await autoReplaceEquipment(gameInstance, false);
                }
            }
            
         
        }
        public async static Task BuyDrugs(MirGameInstanceModel gameInstance, string itemName, int count)
        {
            gameInstance.GameInfo($"购买药品 {itemName} {count}个");
            var nearHome = PickNearHomeMap(gameInstance);  
            var (npcMap, npcName, x, y) = PickDrugNpcByMap(gameInstance, nearHome);
            bool pathFound = await GoRunFunction.PerformPathfinding(CancellationToken.None, gameInstance!, x, y, npcMap, 6);
            if (pathFound)
            {
                await ClickNPC(gameInstance!, npcName);
                await Talk2(gameInstance!, "@buy");
                await Task.Delay(500);

                // 已经检测过存在了, 只看是否为空先
                await BuyDrug(gameInstance, itemName, count);
            }
        }

        
        public async static Task SaveItem(MirGameInstanceModel gameInstance, string npcName, int x, int y, ItemModel[] items, string mapId = "")
        {
            gameInstance.GameInfo($"保存物品 远程执行");
            // bool pathFound = await GoRunFunction.PerformPathfinding(CancellationToken.None, gameInstance!, x, y, "", 6);
            // if (pathFound)
            // {
            //     await ClickNPC(gameInstance!, npcName);
            // }
                await Talk2(gameInstance!, "@storage");

                foreach (var item in items)
                {
                    var data = Common.Utils.StringUtils.GenerateMixedData(
                        item.Name,
                        item.Id
                    );

                    SendMirCall.Send(gameInstance!, 3015, data);
                    await Task.Delay(200);
                }

                await RefreshPackages(gameInstance);

        }
        public static ItemModel? checkReplacementInBag(MirGameInstanceModel instance, EquipPosition position, bool careJPDurability = true)
        {
            var CharacterStatus = instance.CharacterStatus;
            var bagItems = instance.Items;
            // 直接查就可以 因为是替代品不需要很好, 只要关注是不是极品低耐被过滤
            var preferItems = preferStdEquipment(instance, position);
            var final = bagItems.FirstOrDefault(o => !o.IsEmpty && preferItems.Contains(o.Name) && (careJPDurability && o.IsGodly ? !o.IsLowDurability : true));
            return final;
        }
        // 从不主动脱下来, 只会被换
        public async static Task autoReplaceEquipment(MirGameInstanceModel instance, bool careJPDurability = true)
        {
            // instance.GameDebug("开始检查装备更换");

            var CharacterStatus = instance.CharacterStatus;
            var bagItems = instance.Items;
            // 除了蜡烛, 其他装备都检查, 蜡烛是为了保活的, 在另外的函数
            for (int index = 0; index < CharacterStatus.useItems.Count; index++)
            {
                var useItem = CharacterStatus.useItems[index];
                if (index == (int)EquipPosition.RightHand)
                {
                    continue;
                }   
                // 低耐久 极品可被视为无 可以被替换
                var preferItems = CheckPreferComparedUsed(instance, (EquipPosition)index, careJPDurability);
                if(preferItems == null)
                {
                    // 上面是普通购买用的, 背包中可以继续找更JP的, 但是JP之间不比较
                    if(!useItem.IsEmpty && useItem.IsGodly)
                    {
                        continue;
                    }
                    // 找顶级的
                    var mostPreferItem = preferStdEquipment(instance, (EquipPosition)index)?[0];
                    if(mostPreferItem == null)
                    {
                        continue;
                    }
                    var mostPreferItemInBag = bagItems.Where(o =>!o.IsEmpty && o.IsGodly && o.Name == mostPreferItem && (careJPDurability ? !o.IsLowDurability : true)).FirstOrDefault();
                    if (mostPreferItemInBag == null)
                    {
                        continue;
                    }
                    else
                    {
                        // 复用 让下面处理
                        preferItems = new List<string>(){mostPreferItem};
                    }
                }
                // 从背包里找prefer装备了, 
                var validItems = bagItems.Where(o =>
                !o.IsEmpty &&
                // 用索引加速, 不加也可以的 
                o.stdModeToUseItemIndex.Contains((byte)index)
                // 这是为了防止在外面穿破了极品, 在家里可以穿来简化修理流程, 可以重构 prepareBags 部分
                && (careJPDurability ? !(o.IsGodly && o.IsLowDurability) : true)
                );
                // todo 额外的 真正的条件
                // && o.reqType == 0
                // && o.reqPoints <= CharacterStatus.Level
                // && o.reqPoints > item.reqPoints
                // prefer 
                ItemModel? final = null;
                foreach (var preferItem in preferItems)
                {
                    final = validItems.Where(o => o.Name == preferItem)
                    .OrderBy(o => o.IsGodly ? 0 : 1)
                    // .ThenByDescending(o => o.Duration) 这回一直来回换 特别傻逼
                    .FirstOrDefault();
                    if (final != null)
                    {
                        break;
                    }
                }

                if (final != null)
                {
                    instance.GameInfo("找到更好的装备: {Name}, 准备更换到位置 {Index}", final.Name, index);
                    // 装回检查的位置
                    nint toIndex = index;
                    nint bagGridIndex = final.Index;
                    SendMirCall.Send(instance, 3021, new nint[] { bagGridIndex, toIndex });
                    await Task.Delay(1000);
                    ItemFunction.ReadBag(instance);
                }
            }
           
        }
    }
}
