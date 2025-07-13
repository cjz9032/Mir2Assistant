using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mir2Assistant.Common.Models
{
    public class GameAccountModel
    {
        public string Account { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string CharacterName { get; set; } = string.Empty;
        public bool IsMainControl { get; set; }
        public int? ProcessId { get; set; }
        public bool IsRunning => ProcessId.HasValue && ProcessId.Value > 0;
    }
}