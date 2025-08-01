using System;
using System.Collections.Generic;
using Mir2Assistant.Models.MapPathFinding;
using Mir2Assistant.Services;

namespace Mir2Assistant.Examples
{
    public class MapPathFindingExample
    {
        public static void Run()
        {
            var service = new MapPathFindingService();

            // 示例：从比奇省到沃玛森林
            var fromMapId = "0";  // 比奇省
            var toMapId = "1";    // 沃玛森林

            var path = service.FindPath(fromMapId, toMapId);
            if (path == null)
            {
                Console.WriteLine($"未找到从 {service.GetMapName(fromMapId)} 到 {service.GetMapName(toMapId)} 的路径");
                return;
            }

            Console.WriteLine($"从 {service.GetMapName(fromMapId)} 到 {service.GetMapName(toMapId)} 的路径：");
            foreach (var conn in path)
            {
                Console.WriteLine($"  {service.GetMapName(conn.From.MapId)} ({conn.From.X}, {conn.From.Y}) -> {service.GetMapName(conn.To.MapId)} ({conn.To.X}, {conn.To.Y})");
            }
        }
    }
} 