using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Mir2Assistant.Common.Utils;

/// <summary>
/// 怪物覆盖算法工具类
/// 用于计算3x3正方形覆盖怪物的最优位置
/// </summary>
public static class MonsterCoverageUtils
{
    /// <summary>
    /// 找到一个3x3正方形的中心坐标，使其能覆盖最多的怪物（滑动窗口优化版本）
    /// </summary>
    /// <param name="monstersXY">怪物坐标列表，每个元素为(x, y)坐标</param>
    /// <param name="N">需要覆盖的最少怪物数量（用于过滤，如果最大覆盖数小于N则返回(-1,-1)）</param>
    /// <returns>返回3x3正方形的中心坐标(x, y)，如果最大覆盖数小于N则返回(-1, -1)</returns>
    public static (int x, int y) FindOptimal3x3Square(List<(int x, int y)> monstersXY, int N)
    {
        var stopwatch = Stopwatch.StartNew();
        
        if (monstersXY == null || monstersXY.Count < N || N < 2 || N > 9)
        {
            stopwatch.Stop();
            Console.WriteLine($"FindOptimal3x3Square: 参数验证失败, 耗时: {stopwatch.ElapsedMilliseconds}ms");
            return (-1, -1);
        }

        // 找到坐标范围
        int minX = monstersXY.Min(m => m.x);
        int maxX = monstersXY.Max(m => m.x);
        int minY = monstersXY.Min(m => m.y);
        int maxY = monstersXY.Max(m => m.y);

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        var gridBuildTime = Stopwatch.StartNew();
        // 建立动态大小的网格索引
        int[,] grid = new int[width, height];
        foreach (var monster in monstersXY)
        {
            int gridX = monster.x - minX;
            int gridY = monster.y - minY;
            grid[gridX, gridY] = 1;
        }
        gridBuildTime.Stop();

        var searchTime = Stopwatch.StartNew();
        // 使用滑动窗口算法找到覆盖最多怪物的位置
        // 对于小于3x3的网格，需要特殊处理
        int maxStartY = Math.Max(0, height - 3);
        int maxStartX = Math.Max(0, width - 3);
        
        int maxMonsters = 0;
        int bestX = -1, bestY = -1;

        for (int startY = 0; startY <= maxStartY; startY++)
        {
            // 计算第一列的3x3窗口
            int windowSum = 0;
            for (int x = 0; x < 3 && x < width; x++)
            {
                for (int y = startY; y < startY + 3 && y < height; y++)
                {
                    windowSum += grid[x, y];
                }
            }

            // 检查第一个窗口
            if (windowSum > maxMonsters)
            {
                maxMonsters = windowSum;
                bestX = minX + 1;
                bestY = minY + startY + 1;
            }

            // 滑动窗口向右移动
            for (int startX = 1; startX <= maxStartX; startX++)
            {
                // 移除最左列
                for (int y = startY; y < startY + 3 && y < height; y++)
                {
                    windowSum -= grid[startX - 1, y];
                }

                // 添加最右列
                for (int y = startY; y < startY + 3 && y < height; y++)
                {
                    if (startX + 2 < width)
                    {
                        windowSum += grid[startX + 2, y];
                    }
                }

                // 检查当前窗口
                if (windowSum > maxMonsters)
                {
                    maxMonsters = windowSum;
                    bestX = minX + startX + 1;
                    bestY = minY + startY + 1;
                }
            }
        }

        searchTime.Stop();
        stopwatch.Stop();
        
        // 检查是否找到了满足最少数量要求的位置
        if (maxMonsters >= N)
        {
            Console.WriteLine($"FindOptimal3x3Square: 找到最优结果, 覆盖怪物数: {maxMonsters}, 怪物总数: {monstersXY.Count}, 网格大小: {width}x{height}, " +
                            $"网格构建: {gridBuildTime.ElapsedMilliseconds}ms, 搜索: {searchTime.ElapsedMilliseconds}ms, " +
                            $"总耗时: {stopwatch.ElapsedMilliseconds}ms");
            return (bestX, bestY);
        }
        else
        {
            Console.WriteLine($"FindOptimal3x3Square: 未找到满足条件的结果, 最大覆盖数: {maxMonsters}, 需要: {N}, 怪物总数: {monstersXY.Count}, 网格大小: {width}x{height}, " +
                            $"网格构建: {gridBuildTime.ElapsedMilliseconds}ms, 搜索: {searchTime.ElapsedMilliseconds}ms, " +
                            $"总耗时: {stopwatch.ElapsedMilliseconds}ms");
            return (-1, -1);
        }
    }
}
