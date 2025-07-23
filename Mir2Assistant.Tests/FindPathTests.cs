using Mir2Assistant.Common.Functions;
using System.Diagnostics;
using Xunit;

namespace Mir2Assistant.Tests;

public class FindPathTests
{
    [Fact]
    public void FindPath_ShouldReachDestination_OnSimpleMap()
    {
        // 读文件获取
        //byte[] obstacles = null;
        //var configPath = Path.Combine("G:\\c2\\Mir2Assistant\\Mir2Assistant\\config\\server-define\\unity-config\\mapc-out\\0.mapc");
        //if (File.Exists(configPath))
        //{
        //    // binary
        //    var bytes = File.ReadAllBytes(configPath);
        //    obstacles = bytes;
        //}
        //// obstacles 前2个int 32是宽高
        //var width = BitConverter.ToInt32(obstacles!, 0);
        //var height = BitConverter.ToInt32(obstacles!, 4);
        //// 提取后续data
        //var data = new byte[obstacles.Length - 8];
        //Array.Copy(obstacles, 8, data, 0, data.Length);

        //int startX = 630;
        //int startY = 613;
        //int endX = 100;
        //int endY = 200;

        //var sw = Stopwatch.StartNew();
        //var path = GoRunFunction.FindPathCore(width, height, data, startX, startY, endX, endY);
        //sw.Stop();

        //// 输出路径信息
        //Console.WriteLine($"寻路结果: 起点({startX},{startY}) -> 终点({endX},{endY}), 路径长度: {path.Count}, 耗时: {sw.ElapsedMilliseconds}ms");
        //foreach (var step in path)
        //{
        //    Console.WriteLine($"  -> ({step.x},{step.y}) [方向:{step.dir}, 步数:{step.steps}]");
        //}

        //// 基本断言
        //Assert.NotEmpty(path);

        //// 验证路径的每一步
        //int currentX = startX;
        //int currentY = startY;
        //foreach (var (dir, steps, x, y) in path)
        //{
        //    // 确保步数在有效范围内
        //    Assert.InRange(steps, 1, 2);

        //    // 确保坐标连续
        //    Assert.Equal(x, x);
        //    Assert.Equal(y, y);

        //    // 更新当前位置
        //    currentX = x;
        //    currentY = y;
        //}

        //// 验证是否到达目标
        //Assert.Equal(endX, currentX);
        //Assert.Equal(endY, currentY);

        // 验证性能
        //Assert.True(sw.ElapsedMilliseconds < 1000, $"寻路耗时过长: {sw.ElapsedMilliseconds}ms");
    }
} 