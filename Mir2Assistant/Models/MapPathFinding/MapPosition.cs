using System;

namespace Mir2Assistant.Models.MapPathFinding
{
    public class MapPosition
    {
        public string MapId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is MapPosition other)
            {
                return MapId == other.MapId && X == other.X && Y == other.Y;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MapId, X, Y);
        }

        public override string ToString()
        {
            return $"Map:{MapId} ({X},{Y})";
        }
    }
} 