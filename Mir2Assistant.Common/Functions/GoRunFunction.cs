using Mir2Assistant.Common.Models;
using Mir2Assistant.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Serilog; // 新增Serilog引用
using System.Diagnostics; // 新增Stopwatch引用

namespace Mir2Assistant.Common.Functions;
/// <summary>
/// 走路、跑路、寻路
/// </summary>
public static class GoRunFunction
{
    /// <summary>
    /// 走路跑路
    /// </summary>
    /// <param name="pid"></param>
    /// <param name="type">1走路，2跑路，3骑黑马跑</param>
    /// <param name="x">当前x</param>
    /// <param name="x">当前y</param>
    /// <param name="direct">方向，小键盘数字</param>
    /// <param name="走路参数">搜索 8B00 8B4C24 20获取</param>
    /// <param name="UpdateMsg"></param>
    public static void GoRun(MirGameInstanceModel gameInstance, int x, int y, byte direct, byte type)
    {
        int dir = 0;
        switch (direct)
        {
            case 1:
                dir = 5;
                x -= type;
                y += type;
                break;
            case 2:
                dir = 4;
                y += type;
                break;
            case 3:
                dir = 3;
                x += type;
                y += type;
                break;
            case 4:
                dir = 6;
                x -= type;
                break;
            case 6:
                dir = 2;
                x += type;
                break;
            case 7:
                dir = 7;
                x -= type;
                y -= type;
                break;
            case 8:
                dir = 0;
                y -= type;
                break;
            case 9:
                dir = 1;
                x += type;
                y -= type;
                break;
        }
        int typePara = 0;
        switch (type)
        {
            case 2:
                typePara = 0xbc5;
                break;
            case 3:
                typePara = 0x0BC1;
                break;
            default:
                typePara = 0xbc3;
                break;
        }
        SendMirCall.Send(gameInstance, 1001, new nint[] { x, y, dir, typePara, gameInstance!.MirConfig["角色基址"], gameInstance!.MirConfig["UpdateMsg"] });
    }

    public static (int x, int y) getNextPostion(int x, int y, byte dir, byte steps){

        switch (dir)
        {
            case 5:
                x -= steps;
                y += steps;
                break;
            case 4:
                y += steps;
                break;
            case 3:
                x += steps;
                y += steps;
                break;
            case 6:
                x -= steps;
                break;
            case 2:
                x += steps;
                break;
            case 7:
                x -= steps;
                y -= steps;
                break;
            case 0:
                y -= steps;
                break;
            case 1:
                x += steps;
                y -= steps;
                break;
        }
        return (x, y);
    }


    public static void GoRunAlgorithm(MirGameInstanceModel gameInstance, int x, int y, byte dir, byte steps)
    {
        int typePara = 0;
        switch (steps)
        {
            case 1:
                typePara = 0xbc3;
                break;
            case 2:
                typePara = 0xbc5;
                break;
            default:
                typePara = 0xbc3;
                break;
        }
        var (nextX, nextY) = getNextPostion(x, y, dir, steps);
     
        SendMirCall.Send(gameInstance, 1001, new nint[] { nextX, nextY, dir, typePara, gameInstance!.MirConfig["角色基址"], gameInstance!.MirConfig["UpdateMsg"] });
    }


    /// <summary>
    /// 寻路算法
    /// </summary>
    public static List<(byte dir, byte steps, int x, int y)> FindPathCore(
        int width, int height, byte[] obstacleData, 
        int startX, int startY, int targetX, int targetY,
        int maxDistanceBeforeSegment = 300,
        int segmentLength = 120)
    {
        var sw = Stopwatch.StartNew();

        // 如果目的地不能到达
        if (obstacleData[targetY * width + targetX] == 1) {
            sw.Stop();
            Log.Debug($"目标点不可达，耗时: {sw.ElapsedMilliseconds}ms");
            return new List<(byte dir, byte steps, int x, int y)>();
        }

        // 计算总距离
        int totalDistance = Math.Abs(targetX - startX) + Math.Abs(targetY - startY);
        
        // 如果距离超过阈值，使用分段寻路
        if (totalDistance > maxDistanceBeforeSegment)
        {
            Log.Debug($"距离过远({totalDistance}格)，使用分段寻路");
            var finalPath = new List<(byte dir, byte steps, int x, int y)>();
            int currentX = startX;
            int currentY = startY;
            int segmentCount = 0;
            
            while (Math.Abs(targetX - currentX) + Math.Abs(targetY - currentY) > segmentLength)
            {
                segmentCount++;
                var segmentSw = Stopwatch.StartNew();
                // 计算中间点
                int midX = currentX + Math.Sign(targetX - currentX) * segmentLength;
                int midY = currentY + Math.Sign(targetY - currentY) * segmentLength;
                
                // 确保中间点不是障碍物
                bool foundValidMidPoint = false;
                for (int range = 0; range < 10; range++)
                {
                    for (int dx = -range; dx <= range; dx++)
                    {
                        for (int dy = -range; dy <= range; dy++)
                        {
                            int checkX = midX + dx;
                            int checkY = midY + dy;
                            if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height &&
                                obstacleData[checkY * width + checkX] != 1)
                            {
                                midX = checkX;
                                midY = checkY;
                                foundValidMidPoint = true;
                                break;
                            }
                        }
                        if (foundValidMidPoint) break;
                    }
                    if (foundValidMidPoint) break;
                }
                
                if (!foundValidMidPoint)
                {
                    sw.Stop();
                    Log.Warning($"无法找到有效的中间点，总耗时: {sw.ElapsedMilliseconds}ms");
                    return new List<(byte dir, byte steps, int x, int y)>();
                }

                // 寻路到中间点
                var segmentPath = FindPathSingle(width, height, obstacleData, currentX, currentY, midX, midY);
                if (segmentPath.Count == 0)
                {
                    sw.Stop();
                    Log.Warning($"无法找到到中间点({midX},{midY})的路径，总耗时: {sw.ElapsedMilliseconds}ms");
                    return new List<(byte dir, byte steps, int x, int y)>();
                }

                finalPath.AddRange(segmentPath);
                currentX = midX;
                currentY = midY;
                segmentSw.Stop();
                Log.Debug($"完成第{segmentCount}段寻路: ({currentX},{currentY}), 剩余距离: {Math.Abs(targetX - currentX) + Math.Abs(targetY - currentY)}, 本段耗时: {segmentSw.ElapsedMilliseconds}ms");
            }

            // 寻路到最终目标
            var lastSw = Stopwatch.StartNew();
            var lastPath = FindPathSingle(width, height, obstacleData, currentX, currentY, targetX, targetY);
            if (lastPath.Count == 0)
            {
                sw.Stop();
                Log.Warning($"无法找到到最终目标的路径，总耗时: {sw.ElapsedMilliseconds}ms");
                return new List<(byte dir, byte steps, int x, int y)>();
            }

            finalPath.AddRange(lastPath);
            lastSw.Stop();
            sw.Stop();
            Log.Debug($"分段寻路完成，总段数: {segmentCount + 1}, 总步数: {finalPath.Count}, 最后一段耗时: {lastSw.ElapsedMilliseconds}ms, 总耗时: {sw.ElapsedMilliseconds}ms");
            return finalPath;
        }
        else
        {
            var result = FindPathSingle(width, height, obstacleData, startX, startY, targetX, targetY);
            sw.Stop();
            Log.Debug($"普通寻路完成，步数: {result.Count}, 耗时: {sw.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 单段寻路算法
    /// </summary>
    private static List<(byte dir, byte steps, int x, int y)> FindPathSingle(int width, int height, byte[] obstacleData, int startX, int startY, int targetX, int targetY)
    {
        // 方向定义：(dx, dy, 方向值)
        var directions = new[]
        {
            (0, -1, (byte)0),   // 上 8
            (1, -1, (byte)1),   // 右上 9
            (1, 0, (byte)2),    // 右 6
            (1, 1, (byte)3),    // 右下 3
            (0, 1, (byte)4),    // 下 2
            (-1, 1, (byte)5),   // 左下 1
            (-1, 0, (byte)6),   // 左 4
            (-1, -1, (byte)7)   // 左上 7
        };

        // 如果起点和终点相同，返回空路径
        if (startX == targetX && startY == targetY)
        {
            return new List<(byte dir, byte steps, int x, int y)>();
        }

        // 如果目标点就在旁边，直接返回一步路径
        if (Math.Abs(targetX - startX) <= 2 && Math.Abs(targetY - startY) <= 2)
        {
            int dx = Math.Sign(targetX - startX);
            int dy = Math.Sign(targetY - startY);
            byte dir = GetDirectionFromDelta(dx, dy);
            byte steps = (byte)Math.Max(Math.Abs(targetX - startX), Math.Abs(targetY - startY));
            if (steps == 0) steps = 1;
            
            // 检查这一步是否可行
            if (CanMove(startX, startY, dx, dy, steps))
            {
                return new List<(byte dir, byte steps, int x, int y)> { (dir, steps, targetX, targetY) };
            }
        }

        // 使用优先队列来存储待探索的节点
        var openSet = new PriorityQueue<(int x, int y, List<(byte dir, byte steps, int x, int y)> path), int>();
        var visited = new HashSet<string>();
        
        // 将起点加入队列
        openSet.Enqueue((startX, startY, new List<(byte dir, byte steps, int x, int y)>()), 0);
        visited.Add($"{startX},{startY}");

        while (openSet.Count > 0)
        {
            var (currentX, currentY, currentPath) = openSet.Dequeue();

            // 如果到达目标
            if (currentX == targetX && currentY == targetY)
            {
                return currentPath;
            }

            // 计算到目标的方向
            int mainDx = Math.Sign(targetX - currentX);
            int mainDy = Math.Sign(targetY - currentY);

            // 优先检查朝向目标的方向
            var directionsToCheck = directions.OrderBy(d => 
                Math.Abs(d.Item1 - mainDx) + Math.Abs(d.Item2 - mainDy)
            );

            foreach (var (dx, dy, dir) in directionsToCheck)
            {
                // 先尝试2步移动
                for (byte steps = 2; steps >= 1; steps--)
                {
                    if (CanMove(currentX, currentY, dx, dy, steps))
                    {
                        int newX = currentX + dx * steps;
                        int newY = currentY + dy * steps;
                        string newPos = $"{newX},{newY}";

                        // 如果这个位置没访问过
                        if (!visited.Contains(newPos))
                        {
                            visited.Add(newPos);
                            var newPath = new List<(byte dir, byte steps, int x, int y)>(currentPath)
                            {
                                (dir, steps, newX, newY)
                            };

                            // 优先级是到目标的曼哈顿距离
                            int priority = Math.Abs(targetX - newX) + Math.Abs(targetY - newY);
                            openSet.Enqueue((newX, newY, newPath), priority);
                        }
                        break; // 如果能移动，不需要尝试1步
                    }
                }
            }
        }

        // 如果没找到路径
        return new List<(byte dir, byte steps, int x, int y)>();

        // 局部函数：检查是否可以移动
        bool CanMove(int x, int y, int dx, int dy, byte stepSize)
        {
            // 检查目标点是否有效
            int newX = x + dx * stepSize;
            int newY = y + dy * stepSize;
            if (newX < 0 || newX >= width || newY < 0 || newY >= height)
                return false;

            // 检查路径上的所有点
            for (int i = 1; i <= stepSize; i++)
            {
                int checkX = x + dx * i;
                int checkY = y + dy * i;
                if (obstacleData[checkY * width + checkX] == 1)
                    return false;
            }

            return true;
        }
    }

    public static List<(byte dir, byte steps)> genGoPath(MirGameInstanceModel gameInstance, int targetX, int targetY, 
    int[][] monsPos,
    int blurRange = 0,
    bool nearBlur = false,
    int maxDistanceBeforeSegment = 300,
    int segmentLength = 120
    )
    {
        var sw = Stopwatch.StartNew();
        var id = gameInstance!.CharacterStatus!.MapId;
        if (!gameInstance.MapObstacles.TryGetValue(id, out var obstacles)) {
            // 读文件获取
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "config/server-define/unity-config/mapc-out", id + ".mapc");
            if (File.Exists(configPath))
            {
                // binary
                var bytes = File.ReadAllBytes(configPath);
                gameInstance.MapObstacles[id] = bytes;
                obstacles = gameInstance.MapObstacles[id];
            }
        }
        // obstacles 前2个int 32是宽高
        var width = BitConverter.ToInt32(obstacles, 0);
        var height = BitConverter.ToInt32(obstacles, 4);
        int myX = gameInstance!.CharacterStatus!.X;
        int myY = gameInstance!.CharacterStatus!.Y;
        // 提取后续data
        var data = new byte[obstacles.Length - 8];
        Array.Copy(obstacles, 8, data, 0, data.Length);

        // 添加怪物位置作为障碍点
        foreach (var pos in monsPos)
        {
            int monX = pos[0];
            int monY = pos[1];
            if (monX >= 0 && monX < width && monY >= 0 && monY < height)
            {
                data[monY * width + monX] = 1;
            }
        }

        // 检查起点和终点是否为障碍物，若为障碍物则直接返回空路径 
        if (data[targetY * width + targetX] == 1) {
            if (blurRange > 0) {
                List<(int X, int Y)> candidatePoints = new List<(int X, int Y)>();
                
                // 以target为中心，在range范围内选取n*n的范围
                for (int y = targetY - blurRange; y <= targetY + blurRange; y++) {
                    for (int x = targetX - blurRange; x <= targetX + blurRange; x++) {
                        if (x >= 0 && x < width && y >= 0 && y < height) {
                            candidatePoints.Add((x, y));
                        }
                    }
                }
                
                // 根据blurFirst决定排序顺序
                if (nearBlur) {
                    candidatePoints = candidatePoints.OrderBy(p => Math.Abs(p.X - targetX) + Math.Abs(p.Y - targetY)).ToList();
                } else {
                    candidatePoints = candidatePoints.OrderByDescending(p => Math.Abs(p.X - targetX) + Math.Abs(p.Y - targetY)).ToList();
                }
                
                targetX = -1;
                targetY = -1;
                
                if (candidatePoints.Count > 0) {
                    foreach (var point in candidatePoints) {
                        if (data[point.Y * width + point.X] != 1) {
                            targetX = point.X;
                            targetY = point.Y;
                            break;
                        }
                    }
                }
                
                if (targetX == -1) {
                    sw.Stop();
                    Log.Debug($"无法找到有效的模糊目标点，耗时: {sw.ElapsedMilliseconds}ms");
                    return new List<(byte dir, byte steps)>();
                }
            } else {
                sw.Stop();
                Log.Debug($"目标点不可达，耗时: {sw.ElapsedMilliseconds}ms");
                return new List<(byte dir, byte steps)>();
            }
        }

        var path = FindPathCore(width, height, data, myX, myY, targetX, targetY, maxDistanceBeforeSegment, segmentLength);
        sw.Stop();
        Log.Debug($"寻路完成: 起点({myX},{myY}) -> 终点({targetX},{targetY}), 路径长度: {path.Count}, 总耗时(含数据准备): {sw.ElapsedMilliseconds}ms");
        return path.Select(p => (p.dir, p.steps)).ToList();
    }
     

 

    /// <summary>
    /// JPS寻路算法实现
    /// </summary>
    private class Node
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Node? Parent { get; set; }
        public int G { get; set; }
        public int H { get; set; }
        public int F => G + H;
        public byte StepSize { get; set; }
        public byte Direction { get; set; }

        public Node(int x, int y, Node? parent = null)
        {
            X = x;
            Y = y;
            Parent = parent;
            StepSize = 2; // 默认2步移动
        }
    }

    private static byte GetDirectionFromDelta(int dx, int dy)
    {
        if (dx == 0 && dy == -1) return 0;      // 上
        if (dx == 1 && dy == -1) return 1;      // 右上
        if (dx == 1 && dy == 0) return 2;       // 右
        if (dx == 1 && dy == 1) return 3;       // 右下
        if (dx == 0 && dy == 1) return 4;       // 下
        if (dx == -1 && dy == 1) return 5;      // 左下
        if (dx == -1 && dy == 0) return 6;      // 左
        if (dx == -1 && dy == -1) return 7;     // 左上
        return 0;
    }

//     public class PathFindingStats
//     {
//         public int StartX { get; set; }
//         public int StartY { get; set; }
//         public int TargetX { get; set; }
//         public int TargetY { get; set; }
//         public int NodesExplored { get; set; }
//         public int DirectionsChecked { get; set; }
//         public int MovementAttempts { get; set; }
//         public int FailedMoves { get; set; }
//         public int PathLength { get; set; }
//         public int OneStepMoves { get; set; }
//         public int TwoStepMoves { get; set; }
//         public int AlternativePathsUsed { get; set; }
//         public bool NoPathFound { get; set; }
//         public bool PathTooLong { get; set; }
//         public double ElapsedMs { get; set; }

//         public override string ToString()
//         {
//             return $@"寻路统计:
// - 起点: ({StartX}, {StartY}) -> 终点: ({TargetX}, {TargetY})
// - 耗时: {ElapsedMs:F2}ms
// - 探索节点数: {NodesExplored}
// - 检查方向数: {DirectionsChecked}
// - 尝试移动次数: {MovementAttempts}
// - 失败移动次数: {FailedMoves}
// - 路径长度: {PathLength}
//   - 1步移动: {OneStepMoves}
//   - 2步移动: {TwoStepMoves}
// - 使用备选路径次数: {AlternativePathsUsed}
// - 是否找到路径: {!NoPathFound}{(PathTooLong ? " (距离过远)" : "")}";
//         }
//     }

 


    public static async Task<bool> PerformPathfinding(CancellationToken cancellationToken, MirGameInstanceModel GameInstance, int tx, int ty, string replaceMap = "",
          int blurRange = 0,
          bool nearBlur = false
        )
    {

        // todo 跨地图
        CharacterStatusFunction.GetInfo(GameInstance!);
        MonsterFunction.ReadMonster(GameInstance!);

        var stopwatchTotal = new System.Diagnostics.Stopwatch();
        stopwatchTotal.Start();
        
        // gameInstance.Monsters -- 额外的怪物也是障碍点
        var monsterCount = GameInstance!.Monsters.Count;
        int[][] monsPos = new int[monsterCount][];
        int index = 0;
        foreach (var monster in GameInstance!.Monsters)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
            monsPos[index++] = new int[] {
                monster.Value.X,
                monster.Value.Y
            };
        }

        var goNodes = genGoPath(GameInstance!, tx, ty, monsPos, blurRange, nearBlur);
        stopwatchTotal.Stop();
        Log.Debug($"寻路: {stopwatchTotal.ElapsedMilliseconds} 毫秒");
        if (goNodes.Count == 0)
        {
            return false;
        }
        while (goNodes.Count > 0)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }
            var node = goNodes[0];
            goNodes.RemoveAt(0);

            var oldX = GameInstance!.CharacterStatus!.X;
            var oldY = GameInstance!.CharacterStatus!.Y;


            var (nextX, nextY) = getNextPostion(oldX, oldY, node.dir, node.steps);

            GoRunAlgorithm(GameInstance, oldX, oldY, node.dir, node.steps);

            var tried = 0;
            while(true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
                await Task.Delay(100, cancellationToken);
                CharacterStatusFunction.FastUpdateXY(GameInstance!);
                MonsterFunction.ReadMonster(GameInstance!);

                // 执行后发生了变更
                var newX = GameInstance!.CharacterStatus.X;
                var newY = GameInstance!.CharacterStatus.Y;

                tried++;
                if (tried > 8)
                {
                    return await PerformPathfinding(cancellationToken, GameInstance, tx, ty);
                }

                if (oldX != newX || oldY != newY)
                {
                    if (nextX == newX && nextY == newY)
                    {
                        break;
                    } else {
                        // 遇新障了,导致位置不能通过,或偏移，重新执行寻路逻辑
                        return await PerformPathfinding(cancellationToken, GameInstance, tx, ty);
                    }
                }
                // 否则继续等待
            }
        }

        return true;
    }

 


    public static async Task<bool> WaitGoPath(MirGameInstanceModel gameInstance, string map, int x, int y)
    {
        var success = true;
        await Task.Run(() =>
        {
            while (Math.Abs(x - gameInstance.CharacterStatus!.X) > 2 && Math.Abs(y - gameInstance.CharacterStatus!.Y) > 2)
            {
                if (map != gameInstance.CharacterStatus!.MapName)
                {
                    success = false;
                    return;
                }
                // FindPath(gameInstance, x, y);
                Task.Delay(3000).Wait();
            }
        });
        return success;
    }

    public static async Task<bool> FlyCY(MirGameInstanceModel gameInstance)
    {

        SendMirCall.Send(gameInstance, 1010, new nint[] { gameInstance!.MirConfig["通用参数"], gameInstance!.MirConfig["对话CALL地址"] });
        return await NpcFunction.WaitNPC(gameInstance, "天虹法师");
    }

    /// <summary>
    /// 等转到地图
    /// </summary>
    /// <param name="gameInstance"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static async Task<bool> WaitMap(MirGameInstanceModel gameInstance, string mapName, int timeout = 50)
    {
        return await TaskWrapper.Wait(() => gameInstance.CharacterStatus!.MapName == mapName, timeout);
    }
}


