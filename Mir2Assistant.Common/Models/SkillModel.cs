using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mir2Assistant.Common.Models
{
    public class SkillModel
    {
        public int Id { get; set; }
        public int Addr { get; set; }
        public string Name { get; set; } = "";
        public int points { get; set; }
        public int maxPoints { get; set; }
        public int level { get; set; }
        public string Display => $"{Addr?.ToString("x2").PadRight(10)}{Name} {points}/{maxPoints}";
    }
}
