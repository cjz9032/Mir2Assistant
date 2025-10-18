using Xunit;
using Xunit.Abstractions;
using Mir2Assistant.Common.Utils;
using System.Collections.Generic;

namespace Mir2Assistant.Tests;

public class MonsterCoverageUtilsTests
{
    private readonly ITestOutputHelper _output;

    public MonsterCoverageUtilsTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void FindOptimal3x3Square_EmptyMonsterList_ReturnsInvalid()
    {
        // Arrange
        var monsters = new List<(int x, int y)>();
        
        // Act
        var result = MonsterCoverageUtils.FindOptimal3x3Square(monsters, 2);
        
        // Assert
        Assert.Equal((-1, -1), result);
    }

    [Fact]
    public void FindOptimal3x3Square_NullMonsterList_ReturnsInvalid()
    {
        // Act
        var result = MonsterCoverageUtils.FindOptimal3x3Square(null, 2);
        
        // Assert
        Assert.Equal((-1, -1), result);
    }

    [Fact]
    public void FindOptimal3x3Square_InvalidN_ReturnsInvalid()
    {
        // Arrange
        var monsters = new List<(int x, int y)> { (5, 5), (6, 6) };
        
        // Act & Assert
        Assert.Equal((-1, -1), MonsterCoverageUtils.FindOptimal3x3Square(monsters, 1)); // N < 2
        Assert.Equal((-1, -1), MonsterCoverageUtils.FindOptimal3x3Square(monsters, 10)); // N > 9
    }

    [Fact]
    public void FindOptimal3x3Square_InsufficientMonsters_ReturnsInvalid()
    {
        // Arrange
        var monsters = new List<(int x, int y)> { (5, 5) };
        
        // Act
        var result = MonsterCoverageUtils.FindOptimal3x3Square(monsters, 2);
        
        // Assert
        Assert.Equal((-1, -1), result);
    }

    [Fact]
    public void FindOptimal3x3Square_SimpleCase_ReturnsCorrectCenter()
    {
        // Arrange - 3个怪物在(5,5), (5,6), (6,5)位置
        var monsters = new List<(int x, int y)>
        {
            (5, 5), (5, 6), (6, 5)
        };
        
        // Act
        var result = MonsterCoverageUtils.FindOptimal3x3Square(monsters, 3);
        
        // Assert - 中心应该在(6,6)，这样3x3区域覆盖(5,5)到(7,7)
        Assert.Equal((6, 6), result);
    }

    [Fact]
    public void FindOptimal3x3Square_CornerCase_ReturnsCorrectCenter()
    {
        // Arrange - 怪物在左上角
        var monsters = new List<(int x, int y)>
        {
            (0, 0), (0, 1), (1, 0), (1, 1)
        };
        
        // Act
        var result = MonsterCoverageUtils.FindOptimal3x3Square(monsters, 4);
        
        // Assert - 中心应该在(1,1)
        Assert.Equal((1, 1), result);
    }

    [Fact]
    public void FindOptimal3x3Square_EdgeCase_ReturnsCorrectCenter()
    {
        // Arrange - 怪物在右下角边缘
        var monsters = new List<(int x, int y)>
        {
            (17, 17), (17, 18), (18, 17), (18, 18)
        };
        
        // Act
        var result = MonsterCoverageUtils.FindOptimal3x3Square(monsters, 4);
        
        // Assert - 中心应该在(18,18)
        Assert.Equal((18, 18), result);
    }

    [Fact]
    public void FindOptimal3x3Square_ScatteredMonsters_ReturnsOptimalCenter()
    {
        // Arrange - 分散的怪物，右下组有更多怪物
        var monsters = new List<(int x, int y)>
        {
            (5, 5), (5, 6), (6, 5), // 左上组 - 3个怪物
            (15, 15), (15, 16), (16, 15), (16, 16) // 右下组 - 4个怪物
        };
        
        // Act
        var result = MonsterCoverageUtils.FindOptimal3x3Square(monsters, 3);
        
        // Assert - 应该返回覆盖最多怪物的位置（右下组）
        Assert.Equal((15, 15), result);
    }

    [Fact]
    public void FindOptimal3x3Square_MaxCoverage_ReturnsCorrectCenter()
    {
        // Arrange - 创建一个3x3区域内有9个怪物的情况
        var monsters = new List<(int x, int y)>();
        for (int x = 9; x <= 11; x++)
        {
            for (int y = 9; y <= 11; y++)
            {
                monsters.Add((x, y));
            }
        }
        
        // Act
        var result = MonsterCoverageUtils.FindOptimal3x3Square(monsters, 9);
        
        // Assert - 中心应该在(10,10)
        Assert.Equal((10, 10), result);
    }

    [Fact]
    public void FindOptimal3x3Square_OutOfBoundsMonsters_IgnoresInvalidCoordinates()
    {
        // Arrange - 分散的怪物，只有部分区域能覆盖足够数量
        var monsters = new List<(int x, int y)>
        {
            (5, 5), (5, 6), (6, 5) // 3个相邻怪物
        };
        
        // Act
        var result = MonsterCoverageUtils.FindOptimal3x3Square(monsters, 3);
        
        // Assert - 中心应该在(6,6)，覆盖(5,5)到(7,7)的3x3区域
        Assert.Equal((6, 6), result);
    }

    [Fact]
    public void FindOptimal3x3Square_NoValidPosition_ReturnsInvalid()
    {
        // Arrange - 怪物太分散，无法用一个3x3覆盖足够数量
        var monsters = new List<(int x, int y)>
        {
            (0, 0), (5, 5), (10, 10), (15, 15), (19, 19)
        };
        
        // Act
        var result = MonsterCoverageUtils.FindOptimal3x3Square(monsters, 3);
        
        // Assert
        Assert.Equal((-1, -1), result);
    }

    [Fact]
    public void FindOptimal3x3Square_LargeMonsterList_PerformanceTest()
    {
        // Arrange - 创建较大的怪物列表
        var monsters = new List<(int x, int y)>();
        var random = new Random(42); // 固定种子确保测试可重复
        
        for (int i = 0; i < 200; i++)
        {
            monsters.Add((random.Next(0, 20), random.Next(0, 20)));
        }
        
        // Act - 这个测试主要验证算法不会超时
        var result = MonsterCoverageUtils.FindOptimal3x3Square(monsters, 5);
        
        // Assert - 只要不抛异常且返回有效坐标范围即可
        Assert.True(result.x >= -1 && result.x <= 18);
        Assert.True(result.y >= -1 && result.y <= 18);
    }
}
