using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mir2Assistant.Models.MapConnectionFinding;
using Mir2Assistant.Services;

namespace Mir2Assistant.Examples
{
    public class MapPathFindingExample
    {
        public static void Run()
        {
            Console.WriteLine("=== 地图寻路性能测试 ===");
            
            // 测试初始化时间
            var initStopwatch = Stopwatch.StartNew();
            var service = new MapConnectionService();
            initStopwatch.Stop();
            Console.WriteLine($"初始化时间: {initStopwatch.ElapsedMilliseconds}ms ({initStopwatch.ElapsedTicks} ticks)");

            // 测试查询性能
            var testCases = new[]
            {
                ("0", "3"),   // 比奇省 -> 盟重省
                ("0", "1"),   // 比奇省 -> 沃玛森林
                ("3", "11"),  // 盟重省 -> 白日门
                ("0", "0122"), // 比奇省 -> 皇宫
                ("1", "5")    // 沃玛森林 -> 苍月岛
            };

            foreach (var (fromMapId, toMapId) in testCases)
            {
                Console.WriteLine($"\n--- 测试路径: {service.GetMapName(fromMapId)} -> {service.GetMapName(toMapId)} ---");
                
                // 第一次查询（包含计算时间）
                var firstQueryStopwatch = Stopwatch.StartNew();
                var path = service.FindPath(fromMapId, toMapId);
                firstQueryStopwatch.Stop();
                
                Console.WriteLine($"首次查询时间: {firstQueryStopwatch.ElapsedMilliseconds}ms ({firstQueryStopwatch.ElapsedTicks} ticks)");
                
                if (path == null)
                {
                    Console.WriteLine("未找到路径");
                    continue;
                }
                
                Console.WriteLine($"路径长度: {path.Count} 步");
                
                // 第二次查询（缓存命中）
                var cachedQueryStopwatch = Stopwatch.StartNew();
                var cachedPath = service.FindPath(fromMapId, toMapId);
                cachedQueryStopwatch.Stop();
                
                Console.WriteLine($"缓存查询时间: {cachedQueryStopwatch.ElapsedMilliseconds}ms ({cachedQueryStopwatch.ElapsedTicks} ticks)");
                
                // 显示路径详情（只显示前3步）
                Console.WriteLine("路径详情:");
                for (int i = 0; i < Math.Min(3, path.Count); i++)
                {
                    var conn = path[i];
                    Console.WriteLine($"  {i + 1}. {service.GetMapName(conn.From.MapId)} ({conn.From.X}, {conn.From.Y}) -> {service.GetMapName(conn.To.MapId)} ({conn.To.X}, {conn.To.Y})");
                }
                if (path.Count > 3)
                {
                    Console.WriteLine($"  ... 还有 {path.Count - 3} 步");
                }
            }

            // 批量测试
            Console.WriteLine("\n=== 批量性能测试 ===");
            var batchStopwatch = Stopwatch.StartNew();
            int testCount = 0;
            int foundPaths = 0;
            
            var allTestCases = new[]
            {
                ("0", "3"), ("0", "1"), ("0", "11"), ("0", "0122"), ("0", "5"),
                ("3", "0"), ("3", "1"), ("3", "11"), ("3", "5"),
                ("1", "0"), ("1", "3"), ("1", "11"), ("1", "5"),
                ("11", "0"), ("11", "3"), ("11", "1"), ("11", "5")
            };
            
            foreach (var (from, to) in allTestCases)
            {
                var result = service.FindPath(from, to);
                testCount++;
                if (result != null) foundPaths++;
            }
            
            batchStopwatch.Stop();
            Console.WriteLine($"批量测试完成: {testCount} 次查询，{foundPaths} 个有效路径");
            Console.WriteLine($"总耗时: {batchStopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"平均每次查询: {(double)batchStopwatch.ElapsedTicks / testCount:F1} ticks ({(double)batchStopwatch.ElapsedMilliseconds / testCount:F2}ms)");
        }
    }
} 