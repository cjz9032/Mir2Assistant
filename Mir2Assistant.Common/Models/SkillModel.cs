﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mir2Assistant.Common.Models
{
    public class SkillModel
    {
        public int? Addr { get; set; }
        public byte? Length { get; set; }
        public string? Name { get; set; }
        public byte? Type { get; set; }
        public string Display => $"{Addr?.ToString("x2").PadRight(10)}{Name}";
    }
}
