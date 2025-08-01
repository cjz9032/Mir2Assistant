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
            // 创建示例数据
            var connections = new List<MapConnection>
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

            // 创建寻路服务
            var pathFinder = new MapPathFindingService(connections);

            // 测试寻路
            var start = new MapPosition { MapId = "0100", X = 5, Y = 5 };
            var target = new MapPosition { MapId = "0", X = 288, Y = 295 };

            var path = pathFinder.FindNearestPath(start, target);

            if (path != null)
            {
                Console.WriteLine("找到路径:");
                foreach (var connection in path)
                {
                    Console.WriteLine(connection);
                }
            }
            else
            {
                Console.WriteLine("未找到路径");
            }
        }
    }
} 