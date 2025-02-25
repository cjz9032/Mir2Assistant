using Mir2Assistant.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            var memoryUtil = gameInstance!.MemoryUtils!;
            var addr1 = memoryUtil.GetMemoryAddress(gameInstance.MirConfig["对话框基址"], 0);
            var addr2 = memoryUtil.GetMemoryAddress(addr1, 0x11c, 0);
            act();
            nint addr = 0;
            if (!await TaskWrapper.Wait(() =>
            {
                addr = memoryUtil.GetMemoryAddress(addr1, 0x11c, 0);
                return addr2 != addr;
            }))
            {
                return "";
            }
            var length = memoryUtil.ReadToInt(memoryUtil.GetMemoryAddress(addr1, 0x20)) * 3;
            var str = memoryUtil.ReadToString(addr, length);
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
                var npcId = gameInstance.MemoryUtils!.ReadToInt(NPC.Addr + 4);
                SendMirCall.Send(gameInstance, 3001, [npcId, gameInstance!.MirConfig["寻路参数"], gameInstance!.MirConfig["点NPCCALL地址"]]);
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
                SendMirCall.Send(gameInstance, 3002, new nint[] { gameInstance!.MirConfig["买物参数"], gameInstance!.MirConfig["小退参数"],
                gameInstance!.MirConfig["二级对话CALL地址"] ,cmd.Length}.Concat(SendMirCall.String2NIntArray(cmd)).ToArray());
            });
        }

        public static async Task<string> Talk2Text(MirGameInstanceModel gameInstance, string text)
        {
            string? cmd = null;
            if (!await TaskWrapper.Wait(() =>
            {
                cmd = gameInstance.TalkCmds.FirstOrDefault(o => o.Split("/")[0] == text);
                return cmd != null;
            }))
            {
                return "";
            }
            return await Talk2(gameInstance, cmd!.Split("/")[1]);
        }

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
    }
}
