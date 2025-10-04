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
using Serilog;
using Mir2Assistant.Common.Constants; // Add this reference to access DllInject

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
            var base2 = memoryUtil.GetMemoryAddress(GameState.MirConfig["TFrmDlg"], 0);
            var prevAddr2 = memoryUtil.GetMemoryAddress(base2) + GameState.MirConfig["NPC对话偏移"];
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

        public static async Task<string> ClickNPC(MirGameInstanceModel gameInstance, string NpcName, bool fuzzy = true)
        {
            gameInstance.GameDebug("尝试点击NPC: {Name}", NpcName);
            MonsterModel? npc = null;
            if (!await TaskWrapper.Wait(() =>
            {
                npc = gameInstance.Monsters.Values.FirstOrDefault(o => o.TypeStr == "NPC" && (fuzzy ? o.Name.Contains(NpcName) : o.Name == NpcName));
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

        public static async Task Talk2Exit(MirGameInstanceModel gameInstance)
        {
            nint[] data = MemoryUtils.PackStringsToData("@exit");
            SendMirCall.Send(gameInstance, 3002, data);
            await Task.Delay(500);
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
        /// 火把
        /// </summary>
        /// <param name="gameInstance"></param>
        /// <returns></returns>
        public async static Task BuyLZ(MirGameInstanceModel gameInstance, CancellationToken _cancellationToken)
        {
            if (GameState.gamePath == "Client.exe") return;

            gameInstance.GameInfo("购买物品: {Item}, 数量: {Count}", "火把", 1);
            // 查看背包有没火把 没有就买一个
            if (gameInstance.Items.Where(o => o.Name == "火把").Count() < 1)
            {
                // goto npc 
                var nearHome = PickNearHomeMap(gameInstance);
                // 目前仅支持2
                if (nearHome != "2")
                {
                    gameInstance.GameWarning("当前地图不支持购买火把");
                    return;
                }
                var (npcMap, npcName, x, y) = PickMiscNpcByMap(gameInstance, nearHome);
                var pathfound = await GoRunFunction.PerformPathfinding(_cancellationToken, gameInstance, x, y, npcMap, 6);
                if (!pathfound)
                {
                    gameInstance.GameWarning("未找到NPC: {Name}", npcName);
                    return;
                }
                await ClickNPC(gameInstance, npcName);

                nint[] data = MemoryUtils.PackStringsToData("火把");
                await Talk2(gameInstance!, "@buy");
                await Task.Delay(700);
                SendMirCall.Send(gameInstance, 3005, data);
                await Task.Delay(900);
                // 盲选
                for (int i = 0; i < 1; i++)
                {
                    var memoryUtils = gameInstance!.memoryUtils!;
                    var addr = memoryUtils.GetMemoryAddress(GameState.MirConfig["TFrmDlg"], 0xC6C);
                    memoryUtils.WriteInt(addr, i);
                    await Task.Delay(400);
                    SendMirCall.Send(gameInstance, 3006, new nint[] { i });
                    await Task.Delay(300);
                }
                await Talk2Exit(gameInstance!);
            }

        }

        /// <summary>
        /// 直买 药和护身符
        /// </summary>
        /// <param name="gameInstance"></param>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public async static Task BuyImmediate(MirGameInstanceModel gameInstance, string itemName, int count = 1)
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
                gameInstance.memoryUtils.WriteByte(item.addr, 0);
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
        /// 吃书
        /// </summary>
        /// <param name="gameInstance"></param>
        /// <returns></returns>
        public static void EatBookItem(MirGameInstanceModel gameInstance, int id)
        {
            gameInstance.GameDebug("使用书集，id: {id}", id);
            gameInstance.eatItemLastTime = Environment.TickCount;
            SendMirCall.Send(gameInstance, 3029, new nint[] { id });
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
                await Task.Delay(700);
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
            await Task.Delay(600);
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
            if (new string[] { "0", "3" }.Contains(gameInstance.CharacterStatus.MapId)) // except 2
            {
                return gameInstance.CharacterStatus.MapId;
            }
            // 根据当前所在地图, 找到最近的回家点
            var home = "0";
            var mapId = gameInstance.CharacterStatus.MapId;
            // 野外商店
            if (mapId == "D002" || mapId == "DM001")
            {
                home = "DM001";
            }
            // 毒蛇山谷
            if (mapId.StartsWith("2"))
            {
                home = "3";
            }
            if (mapId.StartsWith("D42") || mapId.StartsWith("E6"))
            {
                home = "3";
            }
            // 土城
            // 蜈蚣
            if (mapId.StartsWith("D6") || mapId.StartsWith("E7") || mapId.StartsWith("D5") || mapId.StartsWith("015") || mapId.StartsWith("014"))
            {
                home = "3";
            }
            return home;
        }

        public static (string map, string npcName, int x, int y) PickMiscNpcByMap(MirGameInstanceModel gameInstance, string mapId)
        {
            // 根据当前所在地图, 找到最近的NPC
            if (mapId == "0")
            {
                return ("0", "陈家铺老板", 641, 612);
            }
            else if (mapId == "2")
            {
                return ("2", "罗家铺子老板", 506, 480);
            }
            else if (mapId == "3")
            {
                return ("3", "小贩", 663, 302);
            }
            else if (mapId == "SKILLBB")
            {
                return ("3", "流浪", 349, 334);
            }
            else
            {
                // 其他大图
                return ("-1", "", -1, -1);
            }
        }

        public static (string map, string npcName, int x, int y) PickDrugNpcByMap(MirGameInstanceModel gameInstance, string mapId)
        {
            // 根据当前所在地图, 找到最近的NPC
            if (mapId == "0")
            {
                return ("0", "边界村小店老板", 290, 611);
            }
            if (mapId == "2")
            {
                return ("2", "药铺老板", 506, 496);
            }
            if (mapId == "3")
            {
                return ("0153", "药店", 16, 9);
            }
            if (mapId == "SKILLBB")
            {
                return ("3", "药", 359, 336);
            }
            else
            {
                // 其他大图
                return ("-1", "", 0, 0);
            }
        }
        public static (string map, string npcName, int x, int y) PickBookNpcByMap(MirGameInstanceModel gameInstance, string mapId)
        {
            // 根据当前所在地图, 找到最近的NPC
            if (mapId == "0")
            {
                return ("0132", "书", 6, 18);
            }
            else
            {
                // 其他大图
                return ("-1", "", 0, 0);
            }
        }

        public static (string map, string npcName, int x, int y) PickEquipNpcByMap(MirGameInstanceModel gameInstance, EquipPosition position, string mapId, string action = "buy")
        {
            // 根据当前所在地图, 找到最近的NPC
            if (mapId == "0")
            {
                // 固定为左下角 因为只有这全有买卖, 除了蜡烛
                switch (position)
                {
                    case EquipPosition.Weapon:
                        return ("0", "铁匠", 295, 608);
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
                        return ("-1", "", -1, -1);
                }
            }
            else if (mapId == "2")
            {
                // 固定为左下角 因为只有这全有买卖, 除了蜡烛
                switch (position)
                {
                    case EquipPosition.Weapon:
                        return ("2", "铁匠", 519, 492);
                    case EquipPosition.Dress:
                    case EquipPosition.Helmet:
                        return ("2", "米家服装老板", 518, 478);
                    // case EquipPosition.Necklace:
                    //     return ("0141", "项链店老板", 9, 10);
                    // case EquipPosition.ArmRingLeft:
                    // case EquipPosition.ArmRingRight:
                    //     return ("0141", "手镯店老板", 16, 16);
                    // case EquipPosition.RingLeft:
                    // case EquipPosition.RingRight:
                    //     return ("0141", "戒指店老板", 23, 23);
                    default:
                        return ("-1", "", 0, 0);
                }
            }
            else if (mapId == "3")
            {
                // 固定为左下角 因为只有这全有买卖, 除了蜡烛
                switch (position)
                {
                    case EquipPosition.Weapon:
                        return ("0151", "武器", 10, 15);
                    case EquipPosition.Dress:
                        return ("0155", "布店", 13, 11);
                    case EquipPosition.Helmet:
                        return ("0155", "头盔", 13, 11);
                    case EquipPosition.Necklace:
                        return ("0154", "项链", 6, 16);
                    case EquipPosition.ArmRingLeft:
                    case EquipPosition.ArmRingRight:
                        return ("0154", "手镯", 12, 10);
                    case EquipPosition.RingLeft:
                    case EquipPosition.RingRight:
                        return ("0154", "戒指", 6, 16);
                    default:
                        return ("-1", "", 0, 0);
                }
            }
            else if (mapId == "DM001" && action != "buy" && (position == EquipPosition.Weapon || position == EquipPosition.Dress))
            {
                // 超级野生NPC
                return ("DM001", "商", 4, 6);
            }
            else
            {
                // 其他大图
                return ("-1", "", 0, 0);
            }
        }


        public async static Task RepairAllEquipment(MirGameInstanceModel gameInstance, CancellationToken _cancellationToken)
        {
            var nearHome = PickNearHomeMap(gameInstance);
            foreach (var position in Enum.GetValues(typeof(EquipPosition)))
            {

                var (npcMap, npcName, x, y) = PickEquipNpcByMap(gameInstance, (EquipPosition)position, nearHome, "repair");
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
                    }
                    await Talk2Exit(gameInstance!);
                    // trigger takeon 
                    await autoReplaceEquipment(gameInstance, false);
                }
            }
            await RefreshPackages(gameInstance);
        }


        public async static Task RepairSingleBodyEquipment(MirGameInstanceModel gameInstance, EquipPosition position)
        {
            var nearHome = PickNearHomeMap(gameInstance);

            var (npcMap, npcName, x, y) = PickEquipNpcByMap(gameInstance, (EquipPosition)position, nearHome, "repair");
            var needRep = CheckNeedRep(gameInstance, gameInstance.CharacterStatus.useItems[(int)position]);
            if (!needRep)
            {
                return;
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
                }
                await Talk2Exit(gameInstance!);
                // trigger takeon 
                await autoReplaceEquipment(gameInstance, false);
            }
            // 修不需要
            // await RefreshPackages(gameInstance);
        }

        public async static Task BuyRepairAllFushen(MirGameInstanceModel gameInstance, CancellationToken _cancellationToken)
        {
            if (!GoRunFunction.CapbilityOfSekeleton(gameInstance))
            {
                return;
            }
            var nearHome = PickNearHomeMap(gameInstance);
            var (npcMap, npcName, x, y) = PickMiscNpcByMap(gameInstance, nearHome);
            // 身上也可能有 但是拆装麻烦 直接忽略 放着用完就好了
            // var usedItems = gameInstance.CharacterStatus.useItems.Where(o => !o.IsEmpty && o.stdMode == 25 && o.Name == "护身符").ToList();
            var items = gameInstance.Items.Where(o => !o.IsEmpty && o.stdMode == 25 && o.Name == "护身符").ToList();
            var allFushen = items.Sum(o => o.Duration);
            var BUY_COUNT = 5;
            // 继续用了, 不然太远了
            if (allFushen >= 150)
            {
                return;
            }
            gameInstance.GameInfo($"修理{npcName}的护身符");

            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationToken, gameInstance!, x, y, npcMap, 6);
            if (pathFound)
            {
                // 背包里可能留了N个/ 先修完再买
                // 查找
                await ClickNPC(gameInstance!, npcName);
                foreach (var item in items)
                {
                    await RepairItem(gameInstance, item);
                }

                await Talk2(gameInstance!, "@buy");
                await Task.Delay(500);
                // await BuyImmediate(gameInstance!, "护身符", BUY_COUNT - items.Count);


                nint[] data = MemoryUtils.PackStringsToData("护身符");
                SendMirCall.Send(gameInstance, 3005, data);
                await Task.Delay(1000);
                // 判断是否存在

                var memoryUtils = gameInstance.memoryUtils!;
                var menuListLen = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(memoryUtils.GetMemoryAddress(GameState.MirConfig["TFrmDlg"],
                (int)GameState.MirConfig["商店菜单偏移1"], (int)GameState.MirConfig["商店菜单偏移2"])));
                if (menuListLen > 0)
                {
                    for (int i = 0; i < BUY_COUNT - items.Count; i++)
                    {
                        var addr = memoryUtils.GetMemoryAddress(GameState.MirConfig["TFrmDlg"], (int)GameState.MirConfig["商店菜单指针偏移"]);
                        memoryUtils.WriteInt(addr, 0);
                        await Task.Delay(600);
                        SendMirCall.Send(gameInstance, 3006, new nint[] { 0 });
                        await Task.Delay(700);
                    }
                }

                await RefreshPackages(gameInstance);
            }
        }

        public async static Task BuyLaoLan(MirGameInstanceModel gameInstance, CancellationToken _cancellationToken)
        {
            if (gameInstance.CharacterStatus.Level < 11)
            {
                return;
            }
            var nearHome = PickNearHomeMap(gameInstance);
            var (npcMap, npcName, x, y) = PickMiscNpcByMap(gameInstance, nearHome);
            // 身上也可能有 但是拆装麻烦 直接忽略 放着用完就好了
            // var usedItems = gameInstance.CharacterStatus.useItems.Where(o => !o.IsEmpty && o.stdMode == 25 && o.Name == "护身符").ToList();
            var items = gameInstance.Items.Concat(gameInstance.QuickItems).Where(o => !o.IsEmpty && o.Name == "地牢逃脱卷").ToList();
            var NEED = 3;
            if (items.Count >= NEED)
            {
                return;
            }
            gameInstance.GameInfo($"购买{npcName}的地牢逃脱卷");

            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationToken, gameInstance!, x, y, npcMap, 6);
            if (pathFound)
            {
                // 背包里可能留了N个/ 先修完再买
                // 查找
                await ClickNPC(gameInstance!, npcName);
                await Talk2(gameInstance!, "@buy");
                await Task.Delay(500);
                await BuyImmediate(gameInstance!, "地牢逃脱卷", NEED - items.Count);
                await Task.Delay(1000);
            }
        }


        public async static Task RepairAllBagsEquipment(MirGameInstanceModel gameInstance, CancellationToken _cancellationToken)
        {
            var nearHome = PickNearHomeMap(gameInstance);
            foreach (var position in new EquipPosition[] { EquipPosition.Weapon, EquipPosition.Dress })
            {
                var items = gameInstance.Items.Where(o => !o.IsEmpty && o.stdModeToUseItemIndex.Length > 0 && o.stdModeToUseItemIndex[0] != 255
                // todo 这里如果是首饰还需要继续优化[0], 目前只修武器和衣服
                 && o.stdModeToUseItemIndex[0] == (byte)position).ToList();
                if (items.Count == 0)
                {
                    continue;
                }
                // bug 要所有的 先不管
                // var needRep = CheckNeedRep(gameInstance, items[0]);
                //if (!needRep)
                //{
                //    continue;
                //}

                // 找背包内的对应的东西, 目前是1 , 保留多个的能力

                gameInstance.GameInfo($"背包内保留的{position}装备: {items.Count}个");
                // 找到对应的NPC
                var (npcMap, npcName, x, y) = PickEquipNpcByMap(gameInstance, (EquipPosition)position, nearHome, "repair");
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
                    await Talk2Exit(gameInstance!);
                }
            }
            await RefreshPackages(gameInstance);
        }
        public async static Task RepairSingleBagsEquipment(MirGameInstanceModel gameInstance, EquipPosition position, CancellationToken _cancellationToken)
        {
            var nearHome = PickNearHomeMap(gameInstance);

            var items = gameInstance.Items.Where(o => !o.IsEmpty && o.stdModeToUseItemIndex.Length > 0 && o.stdModeToUseItemIndex[0] != 255
            // todo 这里如果是首饰还需要继续优化[0], 目前只修武器和衣服
                && o.stdModeToUseItemIndex[0] == (byte)position).ToList();
            if (items.Count == 0)
            {
                return;
            }
            // bug 要所有的 先不管
            // var needRep = CheckNeedRep(gameInstance, items[0]);
            //if (!needRep)
            //{
            //    continue;
            //}

            // 找背包内的对应的东西, 目前是1 , 保留多个的能力

            gameInstance.GameInfo($"背包内保留的{position}装备: {items.Count}个");
            // 找到对应的NPC
            var (npcMap, npcName, x, y) = PickEquipNpcByMap(gameInstance, (EquipPosition)position, nearHome, "repair");
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
                await Talk2Exit(gameInstance!);
            }
            // 好像也不需要
            // await RefreshPackages(gameInstance);
        }
        public async static Task sellLJEquipment(MirGameInstanceModel gameInstance, CancellationToken _cancellationToken)
        {
            var nearHome = PickNearHomeMap(gameInstance);
            // 找到所有的装备除了极品 NPC分组去卖了, 
            var ljequipment = gameInstance.Items.Where(o => !o.IsEmpty &&

            !(o.IsGodly && (o.reqType == 0 ? (
                (o.reqPoints <= 20 ? (
                    o.GodPts > 1
                ) : true)
            ) : true) )
            
             && o.stdModeToUseItemIndex.Length > 0 && o.stdModeToUseItemIndex[0] != 255 && o.stdMode != 30)
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
                        var prefs = preferItems;
                        // 保留多个最好的装备，最多3个
                        var keepItems = new List<ItemModel>();
                        foreach (var preferName in prefs)
                        {
                            var foundItems = lists.Where(o => o.Name == preferName).Take(GameConstants.Items.getKeepWeaponCount(gameInstance.CharacterStatus.Level, gameInstance.AccountInfo.role)).ToList();
                            keepItems.AddRange(foundItems);
                            if (keepItems.Count >= GameConstants.Items.getKeepWeaponCount(gameInstance.CharacterStatus.Level, gameInstance.AccountInfo.role)) break;
                        }

                        // 如果找到了要保留的装备，从卖出列表中移除
                        foreach (var keepItem in keepItems.Take(GameConstants.Items.getKeepWeaponCount(gameInstance.CharacterStatus.Level, gameInstance.AccountInfo.role)))
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
                var (npcMap, npcName, x, y) = PickEquipNpcByMap(gameInstance, position, nearHome, "sell");

                gameInstance.GameInfo($"出售{npcName}的{position}装备");
                bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationToken, gameInstance!, x, y, npcMap, 6);
                if (pathFound)
                {
                    await ClickNPC(gameInstance!, npcName);
                    await Talk2(gameInstance!, "@sell");
                    await Task.Delay(500);
                    await SellItems(gameInstance, lists);
                    await Talk2Exit(gameInstance!);
                }
                else
                {
                    gameInstance.GameInfo($"出售失败 {npcName}的{position}装备");
                }
            }
            await RefreshPackages(gameInstance);
        }

         public async static Task sellSingleLJEquipment(MirGameInstanceModel gameInstance, EquipPosition position, CancellationToken _cancellationToken)
        {
            var nearHome = PickNearHomeMap(gameInstance);
            // 找到所有的装备除了极品 NPC分组去卖了, 
            var ljequipment = gameInstance.Items.Where(o => !o.IsEmpty &&
         !(o.IsGodly && (o.reqType == 0 ? (
                (o.reqPoints <= 20 ? (
                    o.GodPts > 1
                ) : true)
            ) : true) )
             && o.stdModeToUseItemIndex.Length > 0 && o.stdModeToUseItemIndex[0] != 255
             && o.stdModeToUseItemIndex[0] == (byte)position
              && o.stdMode != 30);
            // .GroupBy(o => ); // 可以只0, 因为是同一个NPC
          
            var lists = ljequipment.ToList();
            // 可以保留武器和衣服, 因为最容易破, 其他都卖了
            if (position == EquipPosition.Weapon || position == EquipPosition.Dress)
            {
                // 找到推荐的
                var preferItems = preferStdEquipment(gameInstance, position);
                if (preferItems != null && preferItems.Count > 0)
                {
                    var prefs = preferItems;
                    // 保留多个最好的装备，最多3个
                    var keepItems = new List<ItemModel>();
                    foreach (var preferName in prefs)
                    {
                        var foundItems = lists.Where(o => o.Name == preferName).Take(GameConstants.Items.getKeepWeaponCount(gameInstance.CharacterStatus.Level, gameInstance.AccountInfo.role)).ToList();
                        keepItems.AddRange(foundItems);
                        if (keepItems.Count >= GameConstants.Items.getKeepWeaponCount(gameInstance.CharacterStatus.Level, gameInstance.AccountInfo.role)) break;
                    }

                    // 如果找到了要保留的装备，从卖出列表中移除
                    foreach (var keepItem in keepItems.Take(GameConstants.Items.getKeepWeaponCount(gameInstance.CharacterStatus.Level, gameInstance.AccountInfo.role)))
                    {
                        lists.Remove(keepItem);
                        gameInstance.GameInfo($"保留备用{position}: {keepItem.Name} (IsGodly: {keepItem.IsGodly})");
                    }
                }
            }
            // 保留完可能就空了 跳过该轮
            if (lists.Count == 0)
            {
                return;
            }
            var (npcMap, npcName, x, y) = PickEquipNpcByMap(gameInstance, position, nearHome, "sell");

            gameInstance.GameInfo($"出售{npcName}的{position}装备");
            bool pathFound = await GoRunFunction.PerformPathfinding(_cancellationToken, gameInstance!, x, y, npcMap, 6);
            if (pathFound)
            {
                await ClickNPC(gameInstance!, npcName);
                await Talk2(gameInstance!, "@sell");
                await Task.Delay(500);
                await SellItems(gameInstance, lists);
                await Talk2Exit(gameInstance!);
            }
            else
            {
                gameInstance.GameInfo($"出售失败 {npcName}的{position}装备");
            }
        }

        public static List<string> preferStdEquipment(MirGameInstanceModel gameInstance, EquipPosition position, int? levelParam = null, RoleType? roleParam = null)
        {
            var itemNames = new List<string>();
            var genderStr = gameInstance.AccountInfo.Gender == 1 ? "(男)" : "(女)";
            var level = levelParam ?? gameInstance.CharacterStatus.Level;
            var role = roleParam ?? gameInstance.AccountInfo.role;
            // 自动推荐装备
            switch (position)
            {
                // todo 性别
                case EquipPosition.RightHand:
                    itemNames.Add("火把");
                    break;
                case EquipPosition.Necklace:
                    if (role != RoleType.mage)
                    {
                        if (level >= 3)
                        {
                            itemNames.Add("金项链");
                        }
                        if (level >= 3)
                        {
                            itemNames.Add("传统项链");
                        }
                        if (level >= 17)
                        {
                            if (role == RoleType.blade)
                            {
                                itemNames.Add("魔鬼项链");
                            }
                            else if (role == RoleType.taoist)
                            {
                                itemNames.Add("凤凰明珠");
                            }
                        }
                        if (level >= 24)
                        {
                            if (role == RoleType.blade)
                            {
                                itemNames.Add("蓝翡翠项链");
                            }
                            else if (role == RoleType.taoist)
                            {
                                itemNames.Add("竹笛");
                            }
                        }
                    }
                    else
                    {
                        if (level >= 17)
                        {
                            itemNames.Add("琥珀项链");
                        }
                        if (level >= 26)
                        {
                            itemNames.Add("白金项链");
                        }
                        if (level >= 24)
                        {
                            itemNames.Add("放大镜");
                        }
                    }

                    break;
                case EquipPosition.ArmRingLeft:
                case EquipPosition.ArmRingRight:
                    if (level >= 3)
                    {
                        itemNames.Add("铁手镯");
                    }
                    if (level >= 7)
                    {
                        itemNames.Add("皮制手套");
                    }
                    if (level >= 8)
                    {
                        itemNames.Add("钢手镯");
                    }
                    if (level >= 9)
                    {
                        itemNames.Add("大手镯");
                    }
                    if (role == RoleType.blade)
                    {
                        if (level >= 18)
                        {
                            // itemNames.Add("坚固手套");
                        }
                    }
                    else if (role == RoleType.taoist)
                    {
                        if (level >= 19)
                        {
                            // itemNames.Add("坚固手套");
                        }
                    }
                    // todo 换图
                    // if (level >= 30)
                    // {
                    //     itemNames.Add("金手镯");
                    // }
                    break;
                case EquipPosition.RingLeft:
                case EquipPosition.RingRight:
                    if (level >= 3)
                    {
                        itemNames.Add("古铜戒指");
                    }
                    if (level >= 9)
                    {
                        itemNames.Add("牛角戒指");
                    }
                    if (role != RoleType.mage)
                    {
                        if (level >= 16)
                        {
                            itemNames.Add("蓝色水晶戒指");
                        }
                        if (role == RoleType.blade)
                        {
                            if (level >= 20)
                            {
                                itemNames.Add("黑色水晶戒指");
                            }
                            if (level >= 25)
                            {
                                itemNames.Add("珊瑚戒指");
                            }
                        }
                        else if (role == RoleType.taoist)
                        {
                            if (level >= 20)
                            {
                                itemNames.Add("珍珠戒指");
                            }
                            if (level >= 23)
                            {
                                itemNames.Add("道德戒指");
                            }
                        }
                    }
                    else
                    {
                        if (level >= 11)
                        {
                            itemNames.Add("六角戒指");
                        }
                        if (level >= 20)
                        {
                            itemNames.Add("蛇眼戒指");
                        }
                        if (level >= 26)
                        {
                            itemNames.Add("生铁戒指");
                        }
                    }
                    // todo 换图
                    if (level >= 26 && role == RoleType.taoist)
                    {
                        itemNames.Add("金戒指");
                        itemNames.Add("降妖除魔戒指");
                    }
                    break;
                case EquipPosition.Helmet:
                    if (role == RoleType.blade)
                    {
                        if (level >= 10)
                        {
                            itemNames.Add("青铜头盔");
                        }
                        if (level >= 14)
                        {
                            itemNames.Add("魔法头盔");
                        }
                        if (level >= 23)
                        {
                            itemNames.Add("道士头盔");
                        }
                        if (level >= 25)
                        {
                            itemNames.Add("骷髅头盔");
                        }
                    }
                    else
                    {
                        var isMage = role == RoleType.mage;
                        if (isMage)
                        {
                            if (level >= 14)
                            {
                                // itemNames.Add("青铜头盔");
                                // itemNames.Add("魔法头盔");
                            }
                            if (level >= 23)
                            {
                                itemNames.Add("道士头盔");
                            }
                        }
                        else
                        {
                            if (level >= 20)
                            {
                                // itemNames.Add("青铜头盔");
                                itemNames.Add("魔法头盔");
                            }
                            if (level >= 23)
                            {
                                itemNames.Add("道士头盔");
                            }
                        }

                    }


                    break;
                case EquipPosition.Dress:
                    itemNames.Add("布衣" + genderStr);
                    if (role != RoleType.mage)
                    {
                        if (level >= 11)
                        {
                            itemNames.Add("轻型盔甲" + genderStr);
                        }
                        if (level >= 16)
                        {
                            itemNames.Add("中型盔甲" + genderStr);
                        }
                        if (role == RoleType.blade)
                        {
                            if (level >= 22)
                            {
                                itemNames.Add("重盔甲" + genderStr);
                            }
                        }
                        else
                        {
                            if (level >= 22)
                            {
                                itemNames.Add("灵魂战衣" + genderStr);
                            }
                        }

                    }
                    else
                    {
                        if (level >= 11)
                        {
                            itemNames.Add("轻型盔甲" + genderStr);
                        }
                        if (level >= 20)
                        {
                            itemNames.Add("中型盔甲" + genderStr);
                        }
                        if (level >= 22)
                        {
                            itemNames.Add("魔法长袍" + genderStr);
                        }
                    }
                    break;
                case EquipPosition.Weapon:
                    itemNames.Add("木剑");
                    if (role != RoleType.mage)
                    {
                        if (level >= 2)
                        {
                            itemNames.Add("匕首");
                        }
                        if (level >= 5)
                        {
                            itemNames.Add("青铜剑");
                        }
                        if (level < 5)
                        {
                            itemNames.Add("乌木剑");
                        }
                        if (level >= 10)
                        {
                            itemNames.Add("铁剑");
                            itemNames.Add("短剑");
                        }
                        if (level >= 13)
                        {
                            itemNames.Add("青铜斧");
                        }
                        if (level >= 15)
                        {
                            if (role == RoleType.blade)
                            {
                                // itemNames.Add("半月");
                                itemNames.Add("八荒");
                            }
                            else
                            {
                                itemNames.Add("半月");
                            }
                        }
                        if (role == RoleType.blade)
                        {
                            if (level >= 19)
                            {
                                itemNames.Add("凌风");
                            }
                            if (level >= 20)
                            {
                                itemNames.Add("破魂");
                            }
                            if (level >= 22)
                            {
                                itemNames.Add("修罗");
                            }
                        }
                        if (role == RoleType.taoist)
                        {
                            if (level >= 20)
                            {
                                itemNames.Add("破魂");
                                itemNames.Add("降魔");
                            }
                        }
                    }
                    else
                    {
                        // if (level >= 5)
                        // {
                        //     itemNames.Add("青铜剑");
                        // }
                        if (level >= 15)
                        {
                            itemNames.Add("海魂");
                        }
                        if (level >= 20)
                        {
                            itemNames.Add("偃月");
                        }
                    }
                    break;
            }
            itemNames.Reverse();
            return itemNames;
        }


        public async static Task buyAllEquipment(MirGameInstanceModel gameInstance, CancellationToken _cancellationToken)
        {
            if (gameInstance.CharacterStatus.coin < 100) return;
            var lowCoin = gameInstance.CharacterStatus.coin < 2000;
            foreach (var position in Enum.GetValues(typeof(EquipPosition)))
            {
                var preferBuyItems = CheckPreferComparedUsed(gameInstance, (EquipPosition)position);
                if (lowCoin && !((EquipPosition)position == EquipPosition.Dress || (EquipPosition)position == EquipPosition.Weapon)) continue;
                // 看看修不修 卖不卖 , 
                // 买或不买 都可能修
                // 修
                if (preferBuyItems != null && preferBuyItems.Count > 0)
                {
                    var nearHome = PickNearHomeMap(gameInstance);
                    var (npcMap, npcName, x, y) = PickEquipNpcByMap(gameInstance, (EquipPosition)position, nearHome);
                    gameInstance.GameInfo($"购买{position}装备");
                    bool pathFound = await GoRunFunction.PerformPathfinding(CancellationToken.None, gameInstance!, x, y, npcMap, 6);
                    if (pathFound)
                    {
                        var memoryUtils = gameInstance.memoryUtils!;
                        var menuListLen = 0;
                        await ClickNPC(gameInstance!, npcName);
                        await Talk2(gameInstance!, "@buy");
                        await Task.Delay(800);

                        // 从高到低找 , 找不到就用前面的
                        for (int i = 0; i < preferBuyItems.Count; i++)
                        {
                            // 已经检测过存在了, 只看是否为空先
                            var name = preferBuyItems[i];
                            var exists = await CheckExistsInBags(gameInstance, name);
                            if (exists)
                            {
                                break;
                            }

                            nint[] data = MemoryUtils.PackStringsToData(name);
                            SendMirCall.Send(gameInstance, 3005, data);
                            await Task.Delay(1000);
                            // 判断是否存在
                            menuListLen = memoryUtils.ReadToInt(memoryUtils.GetMemoryAddress(memoryUtils.GetMemoryAddress(GameState.MirConfig["TFrmDlg"],
                            (int)GameState.MirConfig["商店菜单偏移1"], (int)GameState.MirConfig["商店菜单偏移2"])));
                            if (menuListLen > 0)
                            {
                                var addr = memoryUtils.GetMemoryAddress(GameState.MirConfig["TFrmDlg"], (int)GameState.MirConfig["商店菜单指针偏移"]);
                                memoryUtils.WriteInt(addr, 0);
                                await Task.Delay(600);
                                SendMirCall.Send(gameInstance, 3006, new nint[] { 0 });
                                await Task.Delay(700);

                                var ss = await CheckExistsInBags(gameInstance, name);
                                if (ss)
                                {
                                    break;
                                }
                            }
                        }
                        await Talk2Exit(gameInstance!);
                        // trigger takeon 
                        await autoReplaceEquipment(gameInstance, false);
                        await RefreshPackages(gameInstance);
                    }
                }

                // 修
                await RepairSingleBodyEquipment(gameInstance, (EquipPosition)position);
                await RefreshPackages(gameInstance);
                await RepairSingleBagsEquipment(gameInstance, (EquipPosition)position, CancellationToken.None);
                await RefreshPackages(gameInstance);
                await sellSingleLJEquipment(gameInstance, (EquipPosition)position, CancellationToken.None);
                await RefreshPackages(gameInstance);
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
                await BuyImmediate(gameInstance, itemName, count);
                await Talk2Exit(gameInstance!);
            }
        }

        public async static Task sellDrugs(MirGameInstanceModel gameInstance, string itemName)
        {

            gameInstance.GameInfo($"出售药品 {itemName}");
            var lists = new List<ItemModel>();
            lists = gameInstance.Items.Concat(gameInstance.QuickItems).Where(x => !x.IsEmpty && x.Name.Contains(itemName)).ToList();
            if (lists.Count == 0) return;
            var nearHome = PickNearHomeMap(gameInstance);
            var (npcMap, npcName, x, y) = PickDrugNpcByMap(gameInstance, nearHome);
            bool pathFound = await GoRunFunction.PerformPathfinding(CancellationToken.None, gameInstance!, x, y, npcMap, 6);
            if (pathFound)
            {
                await ClickNPC(gameInstance!, npcName);
                await Talk2(gameInstance!, "@sell");
                await Task.Delay(500);

                await SellItems(gameInstance, lists);
                await Talk2Exit(gameInstance!);
            }
        }

        public async static Task BuyBook(MirGameInstanceModel gameInstance, string itemName)
        {

            gameInstance.GameInfo($"购买书籍 {itemName}");
            var nearHome = PickNearHomeMap(gameInstance);
            var (npcMap, npcName, x, y) = PickBookNpcByMap(gameInstance, nearHome);
            bool pathFound = await GoRunFunction.PerformPathfinding(CancellationToken.None, gameInstance!, x, y, npcMap, 6);
            if (pathFound)
            {
                await ClickNPC(gameInstance!, npcName);
                await Talk2(gameInstance!, "@buy");
                await Task.Delay(500);

                // 已经检测过存在了, 只看是否为空先
                await BuyImmediate(gameInstance, itemName, 1);
                await Talk2Exit(gameInstance!);
            }
        }

        public async static Task sellAllBook(MirGameInstanceModel gameInstance)
        {

            var lists = new List<ItemModel>();
            lists = gameInstance.Items.Concat(gameInstance.QuickItems).Where(x => !x.IsEmpty && x.stdMode == 4).ToList();
            if (lists.Count == 0) return;
            gameInstance.GameInfo($"卖出书籍 ");
            var nearHome = PickNearHomeMap(gameInstance);
            var (npcMap, npcName, x, y) = PickBookNpcByMap(gameInstance, nearHome);
            bool pathFound = await GoRunFunction.PerformPathfinding(CancellationToken.None, gameInstance!, x, y, npcMap, 6);
            if (pathFound)
            {
                await ClickNPC(gameInstance!, npcName);
                await Talk2(gameInstance!, "@sell");
                await Task.Delay(500);

                await SellItems(gameInstance, lists);
                await Talk2Exit(gameInstance!);
            }
        }

        public async static Task takeOn(MirGameInstanceModel gameInstance, nint itemsIdx, nint toIndex)
        {
            if (toIndex == 255)
            {
                Log.Error($"255 无法装备");
                return;
            }
            var item = gameInstance.QuickItems.Concat(gameInstance.Items).ToList()[(int)itemsIdx];
            if (item == null)
            {
                Log.Error($"物品{itemsIdx} 无法找到");
                return;
            }
            // todo check属性点
            var toPt = GameState.MirConfig["WAITING_USE_ITEM_ADDR"];
            var copySize = GameState.MirConfig["物品SIZE"] / 4; // size
            var data = StringUtils.GenerateMixedData(
                item.Name,
                toIndex,
                item.Id,
                item.addr,
                toPt,
                copySize
            );

            SendMirCall.Send(gameInstance!, 3023, data);
            await Task.Delay(800);
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
                await Task.Delay(500);
            }
            await Talk2Exit(gameInstance!);

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
            if (careJPDurability)
            {
                // 查找附近NPC 如果有 说明是内部 极品不需要换下来
                var npc = instance.Monsters.Values.FirstOrDefault(o => o.TypeStr == "NPC");
                if (npc != null)
                {
                    instance.GameDebug("附近有NPC, 极品不需要换下来");
                    return;
                }
            }
            // instance.GameDebug("开始检查装备更换");

            var CharacterStatus = instance.CharacterStatus;
            var bagItems = instance.Items;
            // 除了蜡烛, 其他装备都检查, 蜡烛是为了保活的, 在另外的函数
            for (int index = 0; index < CharacterStatus.useItems.Count; index++)
            {
                var useItem = CharacterStatus.useItems[index];
                // 低耐久 极品可被视为无 可以被替换
                // JP之间不比较
                if (!useItem.IsEmpty && useItem.IsGodly && (careJPDurability ? !useItem.IsLowDurability : true))
                {
                    continue;
                }
                var preferItems = CheckPreferComparedUsed(instance, (EquipPosition)index, careJPDurability);
                if (preferItems == null)
                {

                    // 找顶级的, 因为没得推了
                    var mostPreferItems = preferStdEquipment(instance, (EquipPosition)index).Take(1).ToList();
                    if (mostPreferItems.Count == 0)
                    {
                        continue;
                    }

                    ItemModel? mostPreferItemInBag = null;
                    foreach (var mostPreferItem in mostPreferItems)
                    {
                        mostPreferItemInBag = bagItems.Where(o => !o.IsEmpty && o.IsGodly && o.Name == mostPreferItem && (careJPDurability ? !o.IsLowDurability : true)).FirstOrDefault();
                        if (mostPreferItemInBag != null)
                        {
                            break;
                        }
                    }


                    if (mostPreferItemInBag == null)
                    {
                        continue;
                    }
                    else
                    {
                        // 复用 让下面处理
                        preferItems = new List<string>() { mostPreferItemInBag.Name };
                    }
                }
                else
                {
                    // 未推荐的可能有JP, 找找前3 JP
                    // 只有2个, 如果有3个就傻了, 因为可能换成非JP
                    ItemModel? mostPreferItemInBag2 = null;
                    var mostPreferItems2 = preferStdEquipment(instance, (EquipPosition)index).Take(3).ToList();

                    foreach (var mostPreferItem2 in mostPreferItems2)
                    {
                        mostPreferItemInBag2 = bagItems.Where(o => !o.IsEmpty && o.IsGodly && o.Name == mostPreferItem2 && (careJPDurability ? !o.IsLowDurability : true)).FirstOrDefault();
                        if (mostPreferItemInBag2 != null)
                        {
                            break;
                        }
                    }
                    if (mostPreferItemInBag2 != null)
                    {
                        preferItems.Add(mostPreferItemInBag2.Name);
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
                    var times = 0;
                    while (true)
                    {
                        times++;
                        if (times > 3)
                        {
                            break;
                        }
                        // 装回检查的位置
                        nint toIndex = index;
                        nint bagGridIndex = final.Index;
                        var fid = final.Id;
                        var fname = final.Name;
                        await takeOn(instance, bagGridIndex + 6, toIndex);
                        await Task.Delay(500);
                        ItemFunction.ReadBag(instance);
                        // 查看有没穿上
                        var useItemAfter = instance.CharacterStatus.useItems[index];
                        if (useItemAfter.Id == fid)
                        {
                            instance.GameInfo("穿上更好的装备: {Name}, 位置 {index}", final.Name, index);
                        }
                        else
                        {
                            SendMirCall.Send(instance, 9011, new nint[] { });
                            await Task.Delay(1000);
                            instance.GameInfo("穿上更好的装备失败: {Name} {name2}, 位置 {index} 重试试试", final.Name, fname, index);
                        }
                    }
                }
            }

        }
    }
}
