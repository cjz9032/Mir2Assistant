using System;

namespace Mir2Assistant.Common.Models.MapPathFinding
{
    public class MapConnection
    {
        public MapPosition From { get; set; }
        public MapPosition To { get; set; }

        public MapConnection()
        {
            From = new MapPosition();
            To = new MapPosition();
        }

        public override string ToString()
        {
            return $"{From} -> {To}";
        }
    }
} 