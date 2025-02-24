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
        public static List<string> GetTaskCmds(string dialog)
        {
            var ret = new List<string>();
            if (string.IsNullOrEmpty(dialog))
            {
                return ret;
            }
            Regex regex = new Regex(@"\<(.*?)\/(.*?)\>");
            Match match = regex.Match(dialog);
            while (match.Success)
            {
                if (match.Value.Contains("/@")){
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
            var addr = memoryUtil.GetMemoryAddress(addr1, 0x11c, 0);
            act();
            int i = 0;
            await Task.Run(() =>
            {
                var addr2 = addr;
                while (addr2 == addr && i < 10)
                {
                    addr2 = memoryUtil.GetMemoryAddress(addr1, 0x11c, 0);
                    Thread.Sleep(100);
                    i++;
                }
                addr = addr2;
            });
            if (i >= 10)
            {
                return "";
            }
            var length = memoryUtil.ReadToInt(memoryUtil.GetMemoryAddress(addr1, 0x20)) * 3;
            var bytes = memoryUtil.ReadToBytes(addr, length);
            return Encoding.GetEncoding("gb2312").GetString(bytes);
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
                SendMirCall.Send(gameInstance, 3001, new nint[] { npcId, gameInstance!.MirConfig["寻路参数"], gameInstance!.MirConfig["点NPCCALL地址"] });
            });
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


    }
}
