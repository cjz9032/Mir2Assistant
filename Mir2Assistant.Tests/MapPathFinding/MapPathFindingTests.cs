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

        [Fact]
        public void TestSquareCoverageAlgorithm()
        {
            System.Console.WriteLine("\n=== 正方形覆盖算法测试 ===");

            // 测试用例1: 单点
            var points1 = new List<(int X, int Y)> { (5, 5) };
            var result1 = Mir2Assistant.Common.Functions.GoRunFunction.FindOptimalSquareCoverage(points1, 1, 3);
            var coverage1 = Mir2Assistant.Common.Functions.GoRunFunction.CalculateTotalCoverage(result1, points1, 3);
            System.Console.WriteLine($"单点测试: 覆盖 {coverage1}/{points1.Count} 个点");
            Assert.Equal(points1.Count, coverage1);

            // 测试用例2: 一条线上的点
            var points2 = new List<(int X, int Y)> { (1, 1), (2, 1), (3, 1), (4, 1), (5, 1) };
            var result2 = Mir2Assistant.Common.Functions.GoRunFunction.FindOptimalSquareCoverage(points2, 2, 3);
            var coverage2 = Mir2Assistant.Common.Functions.GoRunFunction.CalculateTotalCoverage(result2, points2, 3);
            System.Console.WriteLine($"线性点测试: 覆盖 {coverage2}/{points2.Count} 个点，使用 {result2.Count} 个正方形");
            Assert.Equal(points2.Count, coverage2);

            // 测试用例3: 聚集的点
            var points3 = new List<(int X, int Y)> { (5, 5), (5, 6), (6, 5), (6, 6) };
            var result3 = Mir2Assistant.Common.Functions.GoRunFunction.FindOptimalSquareCoverage(points3, 1, 3);
            var coverage3 = Mir2Assistant.Common.Functions.GoRunFunction.CalculateTotalCoverage(result3, points3, 3);
            System.Console.WriteLine($"聚集点测试: 覆盖 {coverage3}/{points3.Count} 个点，使用 {result3.Count} 个正方形");
            Assert.Equal(points3.Count, coverage3);

            // 测试用例4: 分散的点
            var points4 = new List<(int X, int Y)> { (0, 0), (1, 2), (8, 6), (6, 6) };
            var result4 = Mir2Assistant.Common.Functions.GoRunFunction.FindOptimalSquareCoverage(points4, 3, 3);
            var coverage4 = Mir2Assistant.Common.Functions.GoRunFunction.CalculateTotalCoverage(result4, points4, 3);
            System.Console.WriteLine($"分散点测试: 覆盖 {coverage4}/{points4.Count} 个点，使用 {result4.Count} 个正方形");
            Assert.Equal(points4.Count, coverage4);
        }

        [Fact]
        public void TestIsPointInSquare()
        {
            System.Console.WriteLine("\n=== 点在正方形内测试 ===");

            var square = (CenterX: 5, CenterY: 5, Size: 3);
            
            // 应该在内部的点
            var insidePoints = new[] { (5, 5), (4, 4), (6, 6), (3, 5), (7, 5) };
            foreach (var point in insidePoints)
            {
                bool result = Mir2Assistant.Common.Functions.GoRunFunction.IsPointInSquare(point, square, 3);
                System.Console.WriteLine($"点({point.Item1},{point.Item2}) 在正方形内: {result}");
                Assert.True(result);
            }

            // 应该在外部的点
            var outsidePoints = new[] { (2, 5), (8, 5), (5, 2), (5, 8) };
            foreach (var point in outsidePoints)
            {
                bool result = Mir2Assistant.Common.Functions.GoRunFunction.IsPointInSquare(point, square, 3);
                System.Console.WriteLine($"点({point.Item1},{point.Item2}) 在正方形外: {!result}");
                Assert.False(result);
            }
        }
    }
}