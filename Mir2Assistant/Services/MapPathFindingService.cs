using System;
using System.Collections.Generic;
using System.Linq;
using Mir2Assistant.Models.MapPathFinding;

namespace Mir2Assistant.Services
{
    public class MapPathFindingService
    {
        // 存储所有地图ID到索引的映射
        private readonly Dictionary<string, int> _mapToIndex;
        private readonly string[] _indexToMap;
        
        // 存储所有预计算的路径
        private readonly List<MapConnection>[,] _paths;
        
        // 原始连接信息，用于查找具体的传送点
        private readonly Dictionary<string, List<MapConnection>> _adjacencyList;

        public MapPathFindingService(IEnumerable<MapConnection> connections)
        {
            var mapIds = new HashSet<string>();
            _adjacencyList = new Dictionary<string, List<MapConnection>>();

            // 收集所有地图ID并构建邻接表
            foreach (var conn in connections)
            {
                mapIds.Add(conn.From.MapId);
                mapIds.Add(conn.To.MapId);
                
                if (!_adjacencyList.ContainsKey(conn.From.MapId))
                {
                    _adjacencyList[conn.From.MapId] = new List<MapConnection>();
                }
                _adjacencyList[conn.From.MapId].Add(conn);
            }

            // 构建地图ID和索引的映射
            _indexToMap = mapIds.ToArray();
            _mapToIndex = new Dictionary<string, int>();
            for (int i = 0; i < _indexToMap.Length; i++)
            {
                _mapToIndex[_indexToMap[i]] = i;
            }

            // 初始化路径数组
            int n = _indexToMap.Length;
            _paths = new List<MapConnection>[n, n];

            // Floyd-Warshall 算法预处理
            InitializeFloydWarshall(connections);
        }

        private void InitializeFloydWarshall(IEnumerable<MapConnection> connections)
        {
            int n = _indexToMap.Length;

            // 初始化路径数组
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    _paths[i, j] = new List<MapConnection>();
                }
            }

            // 设置直接连接
            foreach (var conn in connections)
            {
                int from = _mapToIndex[conn.From.MapId];
                int to = _mapToIndex[conn.To.MapId];
                _paths[from, to] = new List<MapConnection> { conn };
            }

            // Floyd-Warshall 算法
            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        // 如果通过k能找到一条路径
                        if (i != j && _paths[i, k].Count > 0 && _paths[k, j].Count > 0)
                        {
                            // 如果还没有路径，或者新路径更短
                            if (_paths[i, j].Count == 0 || 
                                _paths[i, k].Count + _paths[k, j].Count < _paths[i, j].Count)
                            {
                                _paths[i, j] = _paths[i, k].Concat(_paths[k, j]).ToList();
                            }
                        }
                    }
                }
            }
        }

        // 新增：只用地图ID的简单路径查找
        public List<MapConnection> FindPath(string fromMapId, string toMapId)
        {
            // 如果是同一地图，返回空路径
            if (fromMapId == toMapId)
            {
                return new List<MapConnection>();
            }

            // 如果地图ID不存在，返回null
            if (!_mapToIndex.ContainsKey(fromMapId) || !_mapToIndex.ContainsKey(toMapId))
            {
                return null;
            }

            // 直接返回预计算的路径
            int fromIndex = _mapToIndex[fromMapId];
            int toIndex = _mapToIndex[toMapId];
            var path = _paths[fromIndex, toIndex];
            
            return path.Count > 0 ? new List<MapConnection>(path) : null;
        }

        // 原有的FindPath方法改名为FindPathWithPosition，并调用新的FindPath方法
        private List<MapConnection> FindPathWithPosition(MapPosition start, MapPosition target)
        {
            return FindPath(start.MapId, target.MapId);
        }

        public List<MapConnection> FindNearestPath(MapPosition start, MapPosition target)
        {
            var path = FindPath(start.MapId, target.MapId);
            if (path == null || !path.Any())
            {
                return path;
            }

            // 更新第一个连接点的起始坐标为实际起始位置
            var firstConnection = path[0];
            firstConnection.From.X = start.X;
            firstConnection.From.Y = start.Y;

            return path;
        }
    }
} 