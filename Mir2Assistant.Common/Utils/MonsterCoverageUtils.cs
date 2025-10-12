using System;
using System.Collections.Generic;

namespace Mir2Assistant.Common.Utils;

/// <summary>
/// 怪物覆盖算法工具类
/// 用于计算3x3正方形覆盖怪物的最优位置
/// </summary>
public static class MonsterCoverageUtils
{
    /// <summary>
    /// 找到一个3x3正方形的中心坐标，使其能覆盖至少N个怪物（优化版本）
    /// </summary>
    /// <param name="monstersXY">怪物坐标列表，每个元素为(x, y)坐标</param>
    /// <param name="N">需要覆盖的最少怪物数量</param>
    /// <returns>返回3x3正方形的中心坐标(x, y)，如果找不到则返回(-1, -1)</returns>
    public static (int x, int y) FindOptimal3x3Square(List<(int x, int y)> monstersXY, int N)
    {
        if (monstersXY == null || monstersXY.Count < N || N < 2 || N > 9)
        {
            return (-1, -1);
        }

        // 建立20x20网格索引，标记哪些位置有怪物
        bool[,] grid = new bool[20, 20];
        foreach (var monster in monstersXY)
        {
            if (monster.x >= 0 && monster.x < 20 && monster.y >= 0 && monster.y < 20)
            {
                grid[monster.x, monster.y] = true;
            }
        }

        // 遍历所有可能的3x3正方形中心位置
        for (int centerX = 1; centerX <= 18; centerX++)
        {
            for (int centerY = 1; centerY <= 18; centerY++)
            {
                int coveredCount = 0;
                
                // 检查3x3区域内的怪物数量（只需要9次查找）
                for (int x = centerX - 1; x <= centerX + 1; x++)
                {
                    for (int y = centerY - 1; y <= centerY + 1; y++)
                    {
                        if (grid[x, y])
                        {
                            coveredCount++;
                        }
                    }
                }
                
                // 如果覆盖的怪物数量达到要求，返回中心坐标
                if (coveredCount >= N)
                {
                    return (centerX, centerY);
                }
            }
        }
        
        // 如果没有找到满足条件的位置
        return (-1, -1);
    }
}
