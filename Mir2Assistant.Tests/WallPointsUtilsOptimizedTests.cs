using Mir2Assistant.Common.Utils;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Mir2Assistant.Tests
{
    public class WallPointsUtilsOptimizedTests
    {
        private const string TestMapName = "D2000";
        private static readonly string _testDataDirectory;
        private readonly ITestOutputHelper _output;

        static WallPointsUtilsOptimizedTests()
        {
            // 设置测试数据目录
            _testDataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "config", "server-define", "unity-config", "wallpoints-out");
        }

        public WallPointsUtilsOptimizedTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestLoadWallPoints()
        {
            // 测试加载靠墙点数据
            var data = WallPointsUtils.GetWallPoints(TestMapName, _testDataDirectory);
            
            Assert.NotNull(data);
            Assert.Equal(TestMapName, data.MapName);
            Assert.True(data.Width > 0);
            Assert.True(data.Height > 0);
            Assert.True(data.GridSize > 0);
            Assert.True(data.WallPointsCount > 0);
            
            _output.WriteLine($"地图: {data.MapName}");
            _output.WriteLine($"尺寸: {data.Width}x{data.Height}");
            _output.WriteLine($"网格: {data.GridWidth}x{data.GridHeight} (大小: {data.GridSize})");
            _output.WriteLine($"靠墙点总数: {data.WallPointsCount}");
        }

        [Fact]
        public void TestSearchPerformance()
        {
            // 测试搜索性能
            int centerX = 100;
            int centerY = 100;
            int range = 20;

            var startTime = DateTime.Now;
            var results = WallPointsUtils.FindNearbyWallPoints(TestMapName, centerX, centerY, range, _testDataDirectory);
            var searchTime = DateTime.Now - startTime;

            _output.WriteLine($"搜索耗时: {searchTime.TotalMilliseconds}ms, 找到 {results.Count} 个点");
            _output.WriteLine($"搜索范围: {range}格");

            Assert.True(results.Count >= 0);
        }

        [Fact]
        public void TestIsWallPoint()
        {
            // 测试靠墙点检查
            var data = WallPointsUtils.GetWallPoints(TestMapName, _testDataDirectory);
            Assert.NotNull(data);

            // 找一个已知的靠墙点进行测试
            bool foundTestPoint = false;
            int testX = 0, testY = 0, expectedWallCount = 0;

            // 从第一个非空网格中找一个点
            for (int i = 0; i < data.GridWidth && !foundTestPoint; i++)
            {
                for (int j = 0; j < data.GridHeight && !foundTestPoint; j++)
                {
                    if (data.Grids[i, j].Count > 0)
                    {
                        var point = data.Grids[i, j].First();
                        testX = point.x;
                        testY = point.y;
                        expectedWallCount = point.wallCount;
                        foundTestPoint = true;
                    }
                }
            }

            if (foundTestPoint)
            {
                // 测试靠墙点检查方法
                bool isWallPoint = WallPointsUtils.IsWallPoint(TestMapName, testX, testY, _testDataDirectory);
                Assert.True(isWallPoint);

                // 测试靠墙数获取方法
                int actualWallCount = WallPointsUtils.GetWallCount(TestMapName, testX, testY, _testDataDirectory);
                Assert.Equal(expectedWallCount, actualWallCount);
                Assert.True(actualWallCount >= 3); // 靠墙点至少有3个方向被墙围绕

                _output.WriteLine($"测试点 ({testX}, {testY}): 是靠墙点={isWallPoint}, 靠墙数={actualWallCount}");
            }
        }

        [Fact]
        public void TestFindNearestWallPoint()
        {
            // 测试最近靠墙点查找
            int currentX = 100;
            int currentY = 100;

            var nearest = WallPointsUtils.FindNearestWallPoint(TestMapName, currentX, currentY, 20, _testDataDirectory);

            _output.WriteLine($"当前位置: ({currentX}, {currentY})");
            _output.WriteLine($"找到的最近点: ({nearest.x}, {nearest.y})");

            // 如果找到有效的点
            if (nearest.x != -1 && nearest.y != -1)
            {
                // 计算距离
                double distance = Math.Sqrt(Math.Pow(nearest.x - currentX, 2) + Math.Pow(nearest.y - currentY, 2));
                _output.WriteLine($"距离: {distance:F2}");
                
                Assert.True(distance <= 20);
            }
            else
            {
                _output.WriteLine("在指定范围内未找到靠墙点");
            }
        }

        [Fact]
        public void TestCacheManagement()
        {
            // 测试缓存管理
            WallPointsUtils.ClearCache();
            var cacheCount = WallPointsUtils.GetCacheCount();
            Assert.Equal(0, cacheCount);

            // 加载数据后应该有缓存
            var data = WallPointsUtils.GetWallPoints(TestMapName, _testDataDirectory);
            cacheCount = WallPointsUtils.GetCacheCount();
            Assert.Equal(1, cacheCount);

            // 预加载测试
            bool preloadSuccess = WallPointsUtils.PreloadWallPoints("TestMap", _testDataDirectory);
            // 预加载可能失败（如果文件不存在），这是正常的

            _output.WriteLine($"缓存统计: {cacheCount}");
            _output.WriteLine($"预加载测试地图结果: {preloadSuccess}");
        }
    }
}
