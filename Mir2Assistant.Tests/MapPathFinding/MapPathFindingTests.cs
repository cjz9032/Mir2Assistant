extern alias cqfz;

using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using MapPosition = cqfz::Mir2Assistant.Models.MapPathFinding.MapPosition;
using MapConnection = cqfz::Mir2Assistant.Models.MapPathFinding.MapConnection;
using MapPathFindingService = cqfz::Mir2Assistant.Services.MapPathFindingService;

namespace Mir2Assistant.Tests.MapPathFinding
{
    public class MapPathFindingTests
    {
        private readonly MapPathFindingService _pathFinder;
        private readonly List<MapConnection> _connections;

        public MapPathFindingTests()
        {
            // 测试初始化时间
            var initStopwatch = Stopwatch.StartNew();
            
            // Setup test data
            _connections = new List<MapConnection>
            {
                new MapConnection
                {
                    From = new MapPosition { MapId = "0101", X = 10, Y = 22 },
                    To = new MapPosition { MapId = "0", X = 287, Y = 296 }
                },
                new MapConnection
                {
                    From = new MapPosition { MapId = "0101", X = 18, Y = 14 },
                    To = new MapPosition { MapId = "0", X = 290, Y = 293 }
                },
                new MapConnection
                {
                    From = new MapPosition { MapId = "0101", X = 3, Y = 19 },
                    To = new MapPosition { MapId = "0100", X = 11, Y = 13 }
                },
                new MapConnection
                {
                    From = new MapPosition { MapId = "0100", X = 11, Y = 14 },
                    To = new MapPosition { MapId = "0101", X = 3, Y = 20 }
                }
            };

            _pathFinder = new MapPathFindingService();
            
            initStopwatch.Stop();
            System.Console.WriteLine($"测试初始化时间: {initStopwatch.ElapsedMilliseconds}ms ({initStopwatch.ElapsedTicks} ticks)");
        }

        [Fact]
        public void FindPath_SameMap_ReturnsEmptyPath()
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Act
            var path = _pathFinder.FindPath("0101", "0101");
            
            stopwatch.Stop();
            System.Console.WriteLine($"FindPath_SameMap 耗时: {stopwatch.ElapsedTicks} ticks ({stopwatch.ElapsedMilliseconds}ms)");

            // Assert
            Assert.NotNull(path);
            Assert.Empty(path);
        }

        [Fact]
        public void FindPath_ValidPath_ReturnsCorrectPath()
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Act
            var path = _pathFinder.FindPath("3", "0");
            
            stopwatch.Stop();
            System.Console.WriteLine($"FindPath_ValidPath (3->0) 耗时: {stopwatch.ElapsedTicks} ticks ({stopwatch.ElapsedMilliseconds}ms)");
            
            if (path != null)
            {
                System.Console.WriteLine($"路径长度: {path.Count} 步");
            }

            // Assert
            Assert.NotNull(path);
        }

        [Fact]
        public void FindPath_InvalidMapId_ReturnsNull()
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Act
            var path = _pathFinder.FindPath("invalid", "0101");
            
            stopwatch.Stop();
            System.Console.WriteLine($"FindPath_InvalidMapId 耗时: {stopwatch.ElapsedTicks} ticks ({stopwatch.ElapsedMilliseconds}ms)");

            // Assert
            Assert.Null(path);
        }

        [Fact]
        public void FindNearestPath_SameMap_ReturnsEmptyPath()
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Act
            var path = _pathFinder.FindNearestPath("0101", "0101");
            
            stopwatch.Stop();
            System.Console.WriteLine($"FindNearestPath_SameMap 耗时: {stopwatch.ElapsedTicks} ticks ({stopwatch.ElapsedMilliseconds}ms)");

            // Assert
            Assert.NotNull(path);
            Assert.Empty(path);
        }

        [Fact]
        public void FindNearestPath_ValidPath_ReturnsPath()
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Act
            var path = _pathFinder.FindNearestPath("0", "1");
            
            stopwatch.Stop();
            System.Console.WriteLine($"FindNearestPath_ValidPath (0->1) 耗时: {stopwatch.ElapsedTicks} ticks ({stopwatch.ElapsedMilliseconds}ms)");
            
            if (path != null)
            {
                System.Console.WriteLine($"路径长度: {path.Count} 步");
            }

            // Assert
            Assert.NotNull(path);
        }

        [Fact]
        public void PerformanceBenchmark()
        {
            System.Console.WriteLine("\n=== 性能基准测试 ===");
            
            var testCases = new[]
            {
                ("0", "3"),   // 比奇省 -> 盟重省
                ("0", "1"),   // 比奇省 -> 沃玛森林
                ("3", "11"),  // 盟重省 -> 白日门
                ("0", "0122"), // 比奇省 -> 皇宫
                ("1", "5")    // 沃玛森林 -> 苍月岛
            };

            foreach (var (from, to) in testCases)
            {
                // 首次查询
                var firstStopwatch = Stopwatch.StartNew();
                var path1 = _pathFinder.FindPath(from, to);
                firstStopwatch.Stop();
                
                // 缓存查询
                var cachedStopwatch = Stopwatch.StartNew();
                var path2 = _pathFinder.FindPath(from, to);
                cachedStopwatch.Stop();
                
                System.Console.WriteLine($"{from}->{to}: 首次 {firstStopwatch.ElapsedTicks} ticks, 缓存 {cachedStopwatch.ElapsedTicks} ticks, 路径长度 {path1?.Count ?? 0}");
            }
            
            // 批量测试
            var batchStopwatch = Stopwatch.StartNew();
            int testCount = 100;
            
            for (int i = 0; i < testCount; i++)
            {
                _pathFinder.FindPath("0", "3");
            }
            
            batchStopwatch.Stop();
            System.Console.WriteLine($"批量测试 {testCount} 次: 总耗时 {batchStopwatch.ElapsedTicks} ticks, 平均 {(double)batchStopwatch.ElapsedTicks / testCount:F1} ticks/次");
        }
    }
} 