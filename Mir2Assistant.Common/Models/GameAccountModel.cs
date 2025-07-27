using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mir2Assistant.Common.Models
{
    public enum RoleType
    {
        blade,
        mage,
        taoist,
    }

    public class GameAccountModel
    {
        public string Account { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Gender { get; set; } = 0; // 0 女 1 男
        public string CharacterName { get; set; } = string.Empty;

        public bool IsMainControl { get; set; }
        public RoleType role { get; set; }
        // 只显示 不要做任何逻辑
        // public int? ProcessId { get; set; }
        public int TaskMain0Step { get; set; }
        public int TaskSub0Step { get; set; }
        // public bool IsRunning => ProcessId.HasValue && ProcessId.Value > 0;
    }
}