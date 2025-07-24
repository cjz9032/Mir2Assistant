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
using Mir2Assistant.Common.Utils; // Add this reference to access DllInject

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
                SendMirCall.Send(gameInstance, 3019, new nint[] { idx });
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
                SendMirCall.Send(gameInstance!, 3020, new nint[] { item.Index });
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
            await Task.Delay(500);
        }
    }
}
