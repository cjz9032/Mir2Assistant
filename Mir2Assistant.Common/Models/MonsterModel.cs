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
        /// <summary>
        /// 0尸体，1自己，2正常
        /// </summary>
        public byte Flag { get; set; }
        public bool isDead { get; set; }
        public bool isButched { get; set; }
        public short Appr { get; set; }
        public string TypeStr
        {
            get
            {
                return Type switch
                {
                    0x32 => "NPC",
                    0x00 => "玩家",
                    0x2d => "守卫",
                    0x0c => "卫士",
                    _ => "(怪)",
                };
            }
        }

        public int? X { get; set; }
        public int? Y { get; set; }
        public string? Name { get; set; }
        /// <summary>
        /// 行会
        /// </summary>
        public string? Guild { get; set; }

        public string Display => $"{$"{X},{Y}",-8}{TypeStr}{(isDead ? "死" : "")} {(isButched ? "已屠宰" : "")} {Name} {Appr} {Addr:x2} {Id:x2}";
    }
}
