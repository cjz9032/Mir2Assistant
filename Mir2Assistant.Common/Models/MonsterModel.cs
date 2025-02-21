using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace Mir2Assistant.Common.Models
{
    public class MonsterModel : INotifyPropertyChanged
    {
        public int Addr { get; set; }
        public short Type { get; set; }
        /// <summary>
        /// 0尸体，1自己，2正常
        /// </summary>
        public byte Flag { get; set; }
        public string TypeStr
        {
            get
            {
                switch (Type)
                {
                    case 0x32:
                        return "NPC";
                    case 0x00:
                        return "玩家";
                    case 0x2d:
                        return "守卫";
                    case 0x0c:
                        return "卫士";
                    default:
                        return "未知(怪)";
                }
            }
        }

        public int? X { get; set; }
        public int? Y { get; set; }
        public string? Name { get; set; }
        /// <summary>
        /// 行会
        /// </summary>
        public string? Guild { get; set; }

        public string Display => $"{$"{X},{Y}".PadRight(8)}{TypeStr}{(Flag == 0 ? "死" : "")}\t{Name}\t{Guild}\t{Addr.ToString("x2")}";

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
