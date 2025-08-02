using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Mir2Assistant.Models.MapConnectionFinding;

namespace Mir2Assistant.Services
{
    public class MapDataLoader
    {
        private class MapEntryJson
        {
            public required List<MapData> list { get; set; }
        }

        private class MapData
        {
            public required string id { get; set; }
            public required string name { get; set; }
            public required List<PositionData> pos { get; set; }
        }

        private class PositionData
        {
            public required PointData from { get; set; }
            public required PointData to { get; set; }
        }

        private class PointData
        {
            public required string mid { get; set; }
            public int x { get; set; }
            public int y { get; set; }
        }

        public static List<MapConnection> LoadMapConnections(string jsonPath)
        {
            var jsonContent = File.ReadAllText(jsonPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var mapEntry = JsonSerializer.Deserialize<MapEntryJson>(jsonContent, options);

            if (mapEntry == null)
            {
                throw new Exception("Failed to deserialize map connections from JSON.");
            }

            var connections = new List<MapConnection>();

            foreach (var map in mapEntry.list)
            {
                foreach (var pos in map.pos)
                {
                    connections.Add(new MapConnection
                    {
                        From = new MapPosition
                        {
                            MapId = pos.from.mid,
                            X = pos.from.x,
                            Y = pos.from.y
                        },
                        To = new MapPosition
                        {
                            MapId = pos.to.mid,
                            X = pos.to.x,
                            Y = pos.to.y
                        }
                    });
                }
            }

            return connections;
        }
    }
} 