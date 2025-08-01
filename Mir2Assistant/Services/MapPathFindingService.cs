using System.Collections.Generic;
using Mir2Assistant.Generated;
using Mir2Assistant.Models.MapPathFinding;

namespace Mir2Assistant.Services
{
    public class MapPathFindingService
    {
        private readonly Dictionary<string, List<MapConnection>> _pathCache = new();

        public MapPathFindingService()
        {
            // 无需初始化 - 图结构已硬编码
        }

        public string GetMapName(string mapId)
        {
            return MapData.MapNames.TryGetValue(mapId, out var name) ? name : mapId;
        }

        public List<MapConnection>? FindPath(string fromMapId, string toMapId)
        {
            var cacheKey = $"{fromMapId}->{toMapId}";
            
            // 检查缓存
            if (_pathCache.TryGetValue(cacheKey, out var cachedPath))
            {
                return cachedPath;
            }

            // 转换为索引
            var fromIndex = MapData.GetMapIndex(fromMapId);
            var toIndex = MapData.GetMapIndex(toMapId);
            
            if (fromIndex == -1 || toIndex == -1)
            {
                return null;
            }

            // 快速 BFS 寻路
            var path = FindPathInternal(fromIndex, toIndex);
            
            // 缓存结果
            _pathCache[cacheKey] = path;
            
            return path;
        }

        private List<MapConnection>? FindPathInternal(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex)
            {
                return new List<MapConnection>();
            }

            var visited = new HashSet<int>();
            var queue = new Queue<(int index, List<(int fromIdx, int[] connData)> path)>();
            queue.Enqueue((fromIndex, new List<(int, int[])>()));
            visited.Add(fromIndex);

            while (queue.Count > 0)
            {
                var (currentIndex, currentPath) = queue.Dequeue();

                if (currentIndex == toIndex)
                {
                    // 转换路径为 MapConnection
                    var result = new List<MapConnection>(currentPath.Count);
                    foreach (var (fromIdx, connData) in currentPath)
                    {
                        result.Add(new MapConnection
                        {
                            From = new MapPosition 
                            { 
                                MapId = MapData.GetMapId(fromIdx), 
                                X = connData[1], 
                                Y = connData[2] 
                            },
                            To = new MapPosition 
                            { 
                                MapId = MapData.GetMapId(connData[0]), 
                                X = connData[3], 
                                Y = connData[4] 
                            }
                        });
                    }
                    return result;
                }

                var connections = MapData.GetConnections(currentIndex);
                foreach (var conn in connections)
                {
                    var nextIndex = conn[0]; // toIndex
                    if (!visited.Contains(nextIndex))
                    {
                        var newPath = new List<(int, int[])>(currentPath) { (currentIndex, conn) };
                        queue.Enqueue((nextIndex, newPath));
                        visited.Add(nextIndex);
                    }
                }
            }

            return null;
        }

        public List<MapConnection>? FindNearestPath(string fromMapId, string toMapId)
        {
            return FindPath(fromMapId, toMapId);
        }
    }
} 