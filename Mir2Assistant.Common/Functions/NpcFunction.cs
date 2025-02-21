using Mir2Assistant.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mir2Assistant.Common.Functions
{
    public static class NpcFunction
    {
        public static void ClickNPC(MirGameInstanceModel gameInstance, MonsterModel NPC)
        {
            var npcId = gameInstance.MemoryUtils!.ReadToInt(NPC.Addr + 4);
            SendMirCall.Send(gameInstance, 3001, new nint[] { npcId, gameInstance!.MirConfig["寻路参数"], gameInstance!.MirConfig["点NPCCALL地址"] });
        }
    }
}
