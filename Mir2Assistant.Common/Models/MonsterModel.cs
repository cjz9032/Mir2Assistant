using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace Mir2Assistant.Common.Models
{
    public class MonsterModel
    {
        public int UpdateId;
        public int Id;
        public int Addr { get; set; }
        public short? Type { get; set; }
        public bool isDead { get; set; }
        public bool isButched { get; set; }
        public short Appr { get; set; }
        public bool isTeams { get; set; }
        public bool isSelf { get; set; }
        public bool isTeamMem { get; set; }
        public bool isTeamMons { get; set; }
        public bool isMyMons { get; set; }
        public string TypeStr
        {
            get
            {
                return Type switch
                {
                    0x32 => "NPC",
                    0x00 => "玩家",
                    0x01 => "玩家", // 挂机的
                    0x2d => "守卫",
                    0x18 => "卫士2",
                    0x0c => "卫士",
                    _ => "(怪)",
                };
            }
        }

        public bool isGuard
        {
            get
            {
                return Type == 0x0c || Type == 0x18;
            }
        }

        public int X { get; set; }
        public int Y { get; set; }


        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public int CurrentMP { get; set; }
        public int MaxMP { get; set; }
        public byte Level { get; set; }
        public string Name { get; set; } = "";
        // public int state = 0;
        // public bool isHidden => state == 0x00800000;
        /// <summary>
        /// 行会
        /// </summary>
        public string? Guild { get; set; }
        public bool stdAliveMon => !isDead && TypeStr == "(怪)" && !isTeamMons;

        public string Display => $"{$"{X},{Y}",-8}{TypeStr}{(isDead ? "死" : "")} {(isButched ? "已屠宰" : "")} {Name} {Appr} {Addr:x2} {Id:x2}";
    }
}
