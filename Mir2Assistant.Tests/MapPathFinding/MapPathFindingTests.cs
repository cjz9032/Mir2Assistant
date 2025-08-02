extern alias cqfz;

using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using MapPosition = cqfz::Mir2Assistant.Models.MapConnectionFinding.MapPosition;
using MapConnection = cqfz::Mir2Assistant.Models.MapConnectionFinding.MapConnection;
using MapConnectionService = cqfz::Mir2Assistant.Services.MapConnectionService;

namespace Mir2Assistant.Tests.MapConnectionFinding
{
    public class MapPathFindingTests
    {
        private readonly MapConnectionService _pathFinder;
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

            _pathFinder = new MapConnectionService();
            
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

        [Fact]
        public void DetailedPerformanceTest()
        {
            System.Console.WriteLine("\n=== 详细性能测试 ===");
            
            var testCases = new[]
            {
                ("0", "3", "比奇省->盟重省"),
                ("0", "1", "比奇省->沃玛森林"), 
                ("0", "11", "比奇省->白日门"),
                ("3", "5", "盟重省->苍月岛"),
                ("1", "11", "沃玛森林->白日门")
            };

            foreach (var (from, to, desc) in testCases)
            {
                System.Console.WriteLine($"\n{desc} ({from}->{to}):");
                
                // 预热
                _pathFinder.FindPath(from, to);
                
                // 测试多次无缓存查询（清除缓存）
                var times = new List<long>();
                for (int i = 0; i < 10; i++)
                {
                    // 创建新的服务实例来避免缓存
                    var freshService = new MapConnectionService();
                    
                    var sw = Stopwatch.StartNew();
                    var path = freshService.FindPath(from, to);
                    sw.Stop();
                    
                    times.Add(sw.ElapsedTicks);
                    if (i == 0 && path != null)
                    {
                        System.Console.WriteLine($"  路径长度: {path.Count} 步");
                    }
                }
                
                var avgTicks = times.Average();
                var minTicks = times.Min();
                var maxTicks = times.Max();
                
                System.Console.WriteLine($"  无缓存查询 (10次):");
                System.Console.WriteLine($"    平均: {avgTicks:F1} ticks ({avgTicks / 10000:F2}ms)");
                System.Console.WriteLine($"    最快: {minTicks} ticks ({minTicks / 10000.0:F2}ms)");
                System.Console.WriteLine($"    最慢: {maxTicks} ticks ({maxTicks / 10000.0:F2}ms)");
                
                // 测试缓存性能
                var cachedTimes = new List<long>();
                for (int i = 0; i < 100; i++)
                {
                    var sw = Stopwatch.StartNew();
                    _pathFinder.FindPath(from, to);
                    sw.Stop();
                    cachedTimes.Add(sw.ElapsedTicks);
                }
                
                var avgCachedTicks = cachedTimes.Average();
                System.Console.WriteLine($"  缓存查询 (100次): 平均 {avgCachedTicks:F1} ticks ({avgCachedTicks / 10000:F2}ms)");
            }
            
            // 极限性能测试
            System.Console.WriteLine($"\n=== 极限性能测试 ===");
            var extremeStopwatch = Stopwatch.StartNew();
            int extremeCount = 1000;
            
            for (int i = 0; i < extremeCount; i++)
            {
                _pathFinder.FindPath("0", "3");
            }
            
            extremeStopwatch.Stop();
            var avgExtremeTicks = (double)extremeStopwatch.ElapsedTicks / extremeCount;
            System.Console.WriteLine($"缓存查询 {extremeCount} 次: 平均 {avgExtremeTicks:F1} ticks ({avgExtremeTicks / 10000:F2}ms)");
        }
    }
} 