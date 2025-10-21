using System;

namespace Mir2Assistant.Models.MapConnectionFinding
{
    public class MapPosition
    {
        public string MapId { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public string NpcName { get; set; } = string.Empty;
        public string NpcCmds { get; set; } = string.Empty;
    }
}