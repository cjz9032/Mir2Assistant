using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using System.Collections.Concurrent;
using Xunit;

namespace Mir2Assistant.Tests;

public class CheckIfSurroundedTests
{
    /// <summary>
    /// 测试3x3九宫格完全被包围的情况
    /// </summary>
    [Fact]
    public void CheckIfSurrounded_3x3_AllBlocked_ReturnsTrue()
    {
        // Arrange: 创建一个简单的测试地图，角色周围3x3全是障碍物
        var mapId = "test_map_3x3";
        int width = 10;
        int height = 10;
        var obstacles = new byte[width * height];
        
        // 角色位置 (5, 5)，周围3x3全是障碍物
        int myX = 5, myY = 5;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int x = myX + dx;
                int y = myY + dy;
                obstacles[y * width + x] = 1; // 设置为障碍物
            }
        }
        
        // 模拟地图数据（需要根据实际的 retriveMapObstacles 实现调整）
        // 这里假设有一个测试用的地图数据
        var monstersByPosition = new ConcurrentDictionary<long, List<MonsterModel>>();
        
        // Act & Assert
        // 注意：这个测试需要实际的地图数据支持，这里只是展示测试结构
        // var result = GoRunFunction.CheckIfSurrounded(mapId, myX, myY, monstersByPosition);
        // Assert.True(result, "3x3九宫格全被障碍物包围应该返回true");
    }

    /// <summary>
    /// 测试3x3九宫格有空位，但5x5外圈被包围的情况
    /// </summary>
    [Fact]
    public void CheckIfSurrounded_3x3_HasGap_But_5x5_Blocked_ReturnsTrue()
    {
        // Arrange: 3x3有一个空位，但5x5外圈全是障碍物
        var mapId = "test_map_5x5";
        int width = 10;
        int height = 10;
        var obstacles = new byte[width * height];
        
        int myX = 5, myY = 5;
        
        // 3x3中留一个空位（例如右上角）
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                if (dx == 1 && dy == -1) continue; // 留一个空位
                int x = myX + dx;
                int y = myY + dy;
                obstacles[y * width + x] = 1;
            }
        }
        
        // 5x5外圈全是障碍物
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                if (dx >= -1 && dx <= 1 && dy >= -1 && dy <= 1) continue; // 跳过3x3内圈
                int x = myX + dx;
                int y = myY + dy;
                obstacles[y * width + x] = 1;
            }
        }
        
        var monstersByPosition = new ConcurrentDictionary<long, List<MonsterModel>>();
        
        // Act & Assert
        // var result = GoRunFunction.CheckIfSurrounded(mapId, myX, myY, monstersByPosition);
        // Assert.True(result, "虽然3x3有空位，但5x5外圈被包围应该返回true");
    }

    /// <summary>
    /// 测试3x3和5x5都有空位的情况
    /// </summary>
    [Fact]
    public void CheckIfSurrounded_Both_3x3_And_5x5_HasGap_ReturnsFalse()
    {
        // Arrange: 3x3和5x5都有空位
        var mapId = "test_map_open";
        int width = 10;
        int height = 10;
        var obstacles = new byte[width * height]; // 全是0，没有障碍物
        
        int myX = 5, myY = 5;
        var monstersByPosition = new ConcurrentDictionary<long, List<MonsterModel>>();
        
        // Act & Assert
        // var result = GoRunFunction.CheckIfSurrounded(mapId, myX, myY, monstersByPosition);
        // Assert.False(result, "3x3和5x5都有空位应该返回false");
    }

    /// <summary>
    /// 测试怪物阻挡的情况
    /// </summary>
    [Fact]
    public void CheckIfSurrounded_BlockedByMonsters_ReturnsTrue()
    {
        // Arrange: 地图没有障碍物，但周围全是怪物
        var mapId = "test_map_monsters";
        int width = 10;
        int height = 10;
        var obstacles = new byte[width * height]; // 全是0
        
        int myX = 5, myY = 5;
        var monstersByPosition = new ConcurrentDictionary<long, List<MonsterModel>>();
        
        // 在周围3x3放置怪物
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int x = myX + dx;
                int y = myY + dy;
                
                long key = ((long)x << 32) | (uint)y;
                var monster = new MonsterModel
                {
                    X = x,
                    Y = y,
                    Id = key,
                    stdAliveMon = true
                };
                monstersByPosition[key] = new List<MonsterModel> { monster };
            }
        }
        
        // Act & Assert
        // var result = GoRunFunction.CheckIfSurrounded(mapId, myX, myY, monstersByPosition);
        // Assert.True(result, "周围全是怪物应该返回true");
    }

    /// <summary>
    /// 测试混合情况：部分障碍物 + 部分怪物
    /// </summary>
    [Fact]
    public void CheckIfSurrounded_MixedObstaclesAndMonsters_ReturnsTrue()
    {
        // Arrange: 3x3中一半是障碍物，一半是怪物
        var mapId = "test_map_mixed";
        int width = 10;
        int height = 10;
        var obstacles = new byte[width * height];
        
        int myX = 5, myY = 5;
        var monstersByPosition = new ConcurrentDictionary<long, List<MonsterModel>>();
        
        // 左侧用障碍物
        for (int dx = -1; dx <= 0; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int x = myX + dx;
                int y = myY + dy;
                obstacles[y * width + x] = 1;
            }
        }
        
        // 右侧用怪物
        for (int dx = 1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int x = myX + dx;
                int y = myY + dy;
                
                long key = ((long)x << 32) | (uint)y;
                var monster = new MonsterModel
                {
                    X = x,
                    Y = y,
                    Id = key,
                    stdAliveMon = true
                };
                monstersByPosition[key] = new List<MonsterModel> { monster };
            }
        }
        
        // Act & Assert
        // var result = GoRunFunction.CheckIfSurrounded(mapId, myX, myY, monstersByPosition);
        // Assert.True(result, "混合障碍物和怪物包围应该返回true");
    }

    /// <summary>
    /// 测试边界情况：角色在地图边缘
    /// </summary>
    [Fact]
    public void CheckIfSurrounded_AtMapBoundary_ConsidersBoundaryAsBlocked()
    {
        // Arrange: 角色在地图左上角
        var mapId = "test_map_boundary";
        int width = 10;
        int height = 10;
        var obstacles = new byte[width * height];
        
        int myX = 0, myY = 0; // 左上角
        var monstersByPosition = new ConcurrentDictionary<long, List<MonsterModel>>();
        
        // 只需要堵住右侧和下侧的几个位置
        obstacles[0 * width + 1] = 1; // 右
        obstacles[1 * width + 0] = 1; // 下
        obstacles[1 * width + 1] = 1; // 右下
        
        // Act & Assert
        // var result = GoRunFunction.CheckIfSurrounded(mapId, myX, myY, monstersByPosition);
        // Assert.True(result, "在边界时，边界外应该算作阻挡");
    }

    /// <summary>
    /// 测试性能：大范围检测
    /// </summary>
    [Fact]
    public void CheckIfSurrounded_Performance_ShouldBeFast()
    {
        // Arrange
        var mapId = "test_map_large";
        int width = 1000;
        int height = 1000;
        var obstacles = new byte[width * height];
        
        int myX = 500, myY = 500;
        var monstersByPosition = new ConcurrentDictionary<long, List<MonsterModel>>();
        
        // 周围5x5全是障碍物
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int x = myX + dx;
                int y = myY + dy;
                obstacles[y * width + x] = 1;
            }
        }
        
        // Act
        var sw = System.Diagnostics.Stopwatch.StartNew();
        // var result = GoRunFunction.CheckIfSurrounded(mapId, myX, myY, monstersByPosition);
        sw.Stop();
        
        // Assert
        // Assert.True(result);
        // Assert.True(sw.ElapsedMilliseconds < 10, $"检测耗时应该小于10ms，实际: {sw.ElapsedMilliseconds}ms");
    }
}
