using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mir2Assistant.Common.Utils
{
    /// <summary>
    /// 靠墙点工具类
    /// 用于加载和查询地图中的靠墙点信息
    /// 支持网格索引优化的快速搜索
    /// </summary>
    public static class WallPointsUtils
    {
        /// <summary>
        /// 网格索引数据缓存
        /// </summary>
        private static readonly Dictionary<string, WallPointsData> _cache = new();

        /// <summary>
        /// 靠墙点数据结构
        /// </summary>
        public class WallPointsData
        {
            public string MapName { get; set; } = string.Empty;
            public int Width { get; set; }
            public int Height { get; set; }
            public int GridSize { get; set; }
            public int GridWidth { get; set; }
            public int GridHeight { get; set; }
            public int WallPointsCount { get; set; }
            public List<(int x, int y, int wallCount)>[,] Grids { get; set; }
            
            /// <summary>
            /// 用于快速查找的 HashSet 索引
            /// </summary>
            public HashSet<(int x, int y)> WallPointsSet { get; set; }

            public WallPointsData(int gridWidth, int gridHeight)
            {
                Grids = new List<(int x, int y, int wallCount)>[gridWidth, gridHeight];
                WallPointsSet = new HashSet<(int x, int y)>();
                
                for (int i = 0; i < gridWidth; i++)
                {
                    for (int j = 0; j < gridHeight; j++)
                    {
                        Grids[i, j] = new List<(int x, int y, int wallCount)>();
                    }
                }
            }
        }

        /// <summary>
        /// 8个方向的偏移量
        /// </summary>
        private static readonly (int dx, int dy)[] Directions = new (int dx, int dy)[]
        {
            (-1, -1), (0, -1), (1, -1),  // 上排：左上、上、右上
            (-1,  0),          (1,  0),  // 中排：左、右
            (-1,  1), (0,  1), (1,  1)   // 下排：左下、下、右下
        };

        /// <summary>
        /// 从二进制文件加载靠墙点数据
        /// </summary>
        /// <param name="filePath">靠墙点文件路径</param>
        /// <returns>靠墙点数据结构</returns>
        public static WallPointsData LoadWallPointsFromBinary(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"靠墙点文件不存在: {filePath}");
            }

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fileStream))
            {
                // 读取地图基本信息
                int width = reader.ReadInt32();
                int height = reader.ReadInt32();
                int gridSize = reader.ReadInt32();
                int gridWidth = reader.ReadInt32();
                int gridHeight = reader.ReadInt32();
                int wallPointsCount = reader.ReadInt32();

                var data = new WallPointsData(gridWidth, gridHeight)
                {
                    MapName = Path.GetFileNameWithoutExtension(filePath).Replace("wallpoints", ""),
                    Width = width,
                    Height = height,
                    GridSize = gridSize,
                    GridWidth = gridWidth,
                    GridHeight = gridHeight,
                    WallPointsCount = wallPointsCount
                };

                // 读取每个网格的数据
                for (int i = 0; i < gridWidth; i++)
                {
                    for (int j = 0; j < gridHeight; j++)
                    {
                        int pointCount = reader.ReadInt32();
                        for (int k = 0; k < pointCount; k++)
                        {
                            int x = reader.ReadInt32();
                            int y = reader.ReadInt32();
                            int wallCount = reader.ReadInt32();
                            var point = (x, y, wallCount);
                            data.Grids[i, j].Add(point);
                            data.WallPointsSet.Add((x, y));
                        }
                    }
                }

                return data;
            }
        }

        /// <summary>
        /// 获取指定地图的靠墙点数据（带缓存）
        /// </summary>
        /// <param name="mapName">地图名称</param>
        /// <param name="wallPointsDirectory">靠墙点文件目录</param>
        /// <returns>靠墙点数据结构</returns>
        public static WallPointsData? GetWallPoints(string mapName, string? wallPointsDirectory = null)
        {
            // 使用缓存
            if (_cache.ContainsKey(mapName))
            {
                return _cache[mapName];
            }

            // 默认目录
            if (string.IsNullOrEmpty(wallPointsDirectory))
            {
                wallPointsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "config", "server-define", "unity-config", "wallpoints-out");
            }

            var filePath = Path.Combine(wallPointsDirectory, $"{mapName}.wallpoints");
            
            try
            {
                var data = LoadWallPointsFromBinary(filePath);
                _cache[mapName] = data;
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载地图 {mapName} 的靠墙点数据失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 在指定范围内查找靠墙点
        /// </summary>
        /// <param name="mapName">地图名称</param>
        /// <param name="centerX">中心X坐标</param>
        /// <param name="centerY">中心Y坐标</param>
        /// <param name="range">搜索范围</param>
        /// <param name="wallPointsDirectory">靠墙点文件目录</param>
        /// <returns>范围内的靠墙点列表，按距离排序</returns>
        public static List<(int x, int y, double distance, int wallCount)> FindNearbyWallPoints(string mapName, int centerX, int centerY, int range, string? wallPointsDirectory = null)
        {
            var data = GetWallPoints(mapName, wallPointsDirectory);
            if (data == null)
            {
                return new List<(int x, int y, double distance, int wallCount)>();
            }

            var nearbyPoints = new List<(int x, int y, double distance, int wallCount)>();

            // 计算需要检查的网格范围
            int gridSize = data.GridSize;
            int minGridX = Math.Max(0, (centerX - range) / gridSize);
            int maxGridX = Math.Min(data.GridWidth - 1, (centerX + range) / gridSize);
            int minGridY = Math.Max(0, (centerY - range) / gridSize);
            int maxGridY = Math.Min(data.GridHeight - 1, (centerY + range) / gridSize);

            // 遍历相关网格
            for (int gridX = minGridX; gridX <= maxGridX; gridX++)
            {
                for (int gridY = minGridY; gridY <= maxGridY; gridY++)
                {
                    var gridPoints = data.Grids[gridX, gridY];
                    foreach (var point in gridPoints)
                    {
                        // 检查是否在范围内
                        if (Math.Abs(point.x - centerX) <= range && Math.Abs(point.y - centerY) <= range)
                        {
                            double distance = Math.Sqrt(Math.Pow(point.x - centerX, 2) + Math.Pow(point.y - centerY, 2));
                            if (distance <= range)
                            {
                                nearbyPoints.Add((point.x, point.y, distance, point.wallCount));
                            }
                        }
                    }
                }
            }

            return nearbyPoints.OrderBy(item => item.distance).ToList();
        }

        /// <summary>
        /// 检查指定位置是否为靠墙点
        /// </summary>
        /// <param name="mapName">地图名称</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="wallPointsDirectory">靠墙点文件目录</param>
        /// <returns>是否为靠墙点</returns>
        public static bool IsWallPoint(string mapName, int x, int y, string? wallPointsDirectory = null)
        {
            var data = GetWallPoints(mapName, wallPointsDirectory);
            if (data == null)
            {
                return false;
            }

            // 使用 HashSet 进行 O(1) 查找
            return data.WallPointsSet.Contains((x, y));
        }

        /// <summary>
        /// 获取指定位置的靠墙数
        /// </summary>
        /// <param name="mapName">地图名称</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="wallPointsDirectory">靠墙点文件目录</param>
        /// <returns>靠墙数，如果不是靠墙点返回0</returns>
        public static int GetWallCount(string mapName, int x, int y, string? wallPointsDirectory = null)
        {
            var data = GetWallPoints(mapName, wallPointsDirectory);
            if (data == null)
            {
                return 0;
            }

            // 计算网格坐标
            int gridX = x / data.GridSize;
            int gridY = y / data.GridSize;

            // 检查网格坐标是否有效
            if (gridX < 0 || gridX >= data.GridWidth || gridY < 0 || gridY >= data.GridHeight)
            {
                return 0;
            }

            // 在对应网格中查找
            var gridPoints = data.Grids[gridX, gridY];
            var point = gridPoints.FirstOrDefault(p => p.x == x && p.y == y);
            return point.wallCount;
        }

        /// <summary>
        /// 查找最优的靠墙点（优先选择靠墙数最大的，相同靠墙数时选择距离最近的）
        /// </summary>
        /// <param name="mapName">地图名称</param>
        /// <param name="currentX">当前X坐标</param>
        /// <param name="currentY">当前Y坐标</param>
        /// <param name="maxRange">最大搜索范围</param>
        /// <param name="wallPointsDirectory">靠墙点文件目录</param>
        /// <returns>最优的靠墙点坐标，如果没找到返回(-1, -1)</returns>
        public static (int x, int y, int wallCount) FindNearestWallPoint(string mapName, int currentX, int currentY, int maxRange = 10, string? wallPointsDirectory = null)
        {
            var nearbyPoints = FindNearbyWallPoints(mapName, currentX, currentY, maxRange, wallPointsDirectory);
            
            if (nearbyPoints.Count > 0)
            {
                // 按靠墙数降序排序，相同靠墙数时按距离升序排序
                var bestPoint = nearbyPoints
                    .OrderByDescending(p => p.wallCount)  // 优先选择靠墙数最大的
                    .ThenBy(p => p.distance)              // 相同靠墙数时选择距离最近的
                    .First();
                
                return (bestPoint.x, bestPoint.y, bestPoint.wallCount);
            }

            return (-1, -1, 0);
        }

        /// <summary>
        /// 查找指定范围内的所有靠墙点（按靠墙数降序，距离升序排序）
        /// </summary>
        /// <param name="mapName">地图名称</param>
        /// <param name="currentX">当前X坐标</param>
        /// <param name="currentY">当前Y坐标</param>
        /// <param name="maxRange">最大搜索范围</param>
        /// <param name="wallPointsDirectory">靠墙点文件目录</param>
        /// <returns>范围内所有靠墙点列表，按靠墙数降序、距离升序排序</returns>
        public static List<(int x, int y, double distance, int wallCount)> FindAllWallPointsInRange(string mapName, int currentX, int currentY, int maxRange = 10, string? wallPointsDirectory = null)
        {
            var nearbyPoints = FindNearbyWallPoints(mapName, currentX, currentY, maxRange, wallPointsDirectory);
            
            // 按靠墙数降序排序，相同靠墙数时按距离升序排序
            return nearbyPoints
                .OrderByDescending(p => p.wallCount)  // 优先选择靠墙数最大的
                .ThenBy(p => p.distance)              // 相同靠墙数时选择距离最近的
                .ToList();
        }

        /// <summary>
        /// 实时检查指定位置是否为靠墙点（不依赖预生成数据）
        /// </summary>
        /// <param name="obstacles">障碍物数组</param>
        /// <param name="width">地图宽度</param>
        /// <param name="height">地图高度</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns>是否为靠墙点</returns>
        public static bool IsWallPointRealtime(byte[] obstacles, int width, int height, int x, int y)
        {
            // 检查当前位置是否可通行
            if (IsObstacle(obstacles, width, height, x, y))
            {
                return false; // 障碍物位置不能作为靠墙点
            }

            int obstacleCount = 0;
            foreach (var (dx, dy) in Directions)
            {
                if (IsObstacle(obstacles, width, height, x + dx, y + dy))
                {
                    obstacleCount++;
                }
            }

            return obstacleCount >= 3;
        }

        /// <summary>
        /// 检查指定位置是否为障碍物
        /// </summary>
        /// <param name="obstacles">障碍物数组</param>
        /// <param name="width">地图宽度</param>
        /// <param name="height">地图高度</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns>是否为障碍物</returns>
        private static bool IsObstacle(byte[] obstacles, int width, int height, int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return true; // 地图边界视为障碍物
            }
            
            int index = y * width + x;
            return obstacles[index] != 0; // 假设0表示可通行，非0表示障碍物
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public static void ClearCache()
        {
            _cache.Clear();
        }

        /// <summary>
        /// 获取缓存统计信息
        /// </summary>
        /// <returns>缓存的地图数量</returns>
        public static int GetCacheCount()
        {
            return _cache.Count;
        }

        /// <summary>
        /// 预加载指定地图的靠墙点数据
        /// </summary>
        /// <param name="mapName">地图名称</param>
        /// <param name="wallPointsDirectory">靠墙点文件目录</param>
        /// <returns>是否加载成功</returns>
        public static bool PreloadWallPoints(string mapName, string? wallPointsDirectory = null)
        {
            try
            {
                var data = GetWallPoints(mapName, wallPointsDirectory);
                return data != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
