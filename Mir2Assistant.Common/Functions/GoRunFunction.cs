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
 
    public static List<(byte dir, byte steps)> genGoPath(MirGameInstanceModel gameInstance, int targetX, int targetY, 
    int[][] monsPos,
    int blurRange = 0,
    bool nearBlur = false
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

        // 检查起点和终点是否为障碍物
        if (blurRange > 0) {
            List<(int X, int Y, double Distance)> candidatePoints = new List<(int X, int Y, double Distance)>();
                
            // 以target为中心，在range范围内选取n*n的范围
            for (int y = targetY - blurRange; y <= targetY + blurRange; y++) {
                for (int x = targetX - blurRange; x <= targetX + blurRange; x++) {
                    if (x >= 0 && x < width && y >= 0 && y < height && data[y * width + x] != 1) {
                        // 计算到起点和终点的距离
                        double distToStart = Math.Sqrt(Math.Pow(x - myX, 2) + Math.Pow(y - myY, 2));
                        double distToTarget = Math.Sqrt(Math.Pow(x - targetX, 2) + Math.Pow(y - targetY, 2));
                        // 综合距离评分：起点距离权重0.7，终点距离权重0.3
                        double score = nearBlur ? 
                            (99 * distToStart + 0.3 * distToTarget) :  // 近距离优先
                            (0.3 * distToStart + 99 * distToTarget);   // 远距离优先
                            
                        candidatePoints.Add((x, y, score));
                    }
                }
            }
                
            // 根据综合距离评分排序
            candidatePoints = candidatePoints.OrderBy(p => p.Distance).ToList();
                
            targetX = -1;
            targetY = -1;
                
            if (candidatePoints.Count > 0) {
                // 取评分最优的点
                targetX = candidatePoints[0].X;
                targetY = candidatePoints[0].Y;
                Log.Debug($"找到最佳模糊目标点: ({targetX}, {targetY}), 距离评分: {candidatePoints[0].Distance:F2}");
            }
                
            if (targetX == -1) {
                sw.Stop();
                Log.Debug($"无法找到有效的模糊目标点，耗时: {sw.ElapsedMilliseconds}ms");
                return new List<(byte dir, byte steps)>();
            }
        } else if(data[targetY * width + targetX] == 1){
            sw.Stop();
            Log.Debug($"目标点不可达，耗时: {sw.ElapsedMilliseconds}ms");
            return new List<(byte dir, byte steps)>();
        }
        
        // 分距离, 100以内直接a星
        List<(byte dir, byte steps, int x, int y)> path = new List<(byte dir, byte steps, int x, int y)>();
        if (Math.Abs(targetX - myX) + Math.Abs(targetY - myY) <= 150)
        {
            path = FindPathCoreSmallWithAStar(width, height, data, myX, myY, targetX, targetY);
        }
        else
        {
            path = FindPathJPS(width, height, data, myX, myY, targetX, targetY);
        }
        // 优化路径
        path = OptimizePath(path);
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

    private static List<(byte dir, byte steps, int x, int y)> FindPathCoreSmallWithAStar(int width, int height, byte[] obstacles, int startX, int startY, int targetX, int targetY)
    {
        var openSet = new PriorityQueue<Node, int>();
        var closedSet = new HashSet<string>();
        var startNode = new Node(startX, startY);
        startNode.G = 0;
        startNode.H = Math.Abs(targetX - startX) + Math.Abs(targetY - startY);
        openSet.Enqueue(startNode, startNode.F);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current.X == targetX && current.Y == targetY)
            {
                return ReconstructPath(current);
            }

            var key = $"{current.X},{current.Y}";
            if (closedSet.Contains(key)) continue;
            closedSet.Add(key);

            // 8个方向
            int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
            int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

            for (int i = 0; i < 8; i++)
            {
                int newX = current.X + dx[i];
                int newY = current.Y + dy[i];

                if (newX < 0 || newX >= width || newY < 0 || newY >= height) continue;
                if (obstacles[newY * width + newX] == 1) continue;

                var neighbor = new Node(newX, newY, current);
                neighbor.Direction = (byte)i;
                neighbor.StepSize = 1;
                
                // 对角线移动代价和直线移动一样
                neighbor.G = current.G + 1;
                neighbor.H = Math.Abs(targetX - newX) + Math.Abs(targetY - newY);

                if (!closedSet.Contains($"{newX},{newY}"))
                {
                    openSet.Enqueue(neighbor, neighbor.F);
                }
            }
        }

        return new List<(byte dir, byte steps, int x, int y)>();
    }

    private static List<(byte dir, byte steps, int x, int y)> FindPathJPS(int width, int height, byte[] obstacles, int startX, int startY, int targetX, int targetY)
    {
        var openSet = new PriorityQueue<Node, int>();
        var closedSet = new HashSet<string>();
        var startNode = new Node(startX, startY);
        startNode.G = 0;
        startNode.H = Math.Abs(targetX - startX) + Math.Abs(targetY - startY);
        openSet.Enqueue(startNode, startNode.F);

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current.X == targetX && current.Y == targetY)
            {
                return ReconstructPath(current);
            }

            var key = $"{current.X},{current.Y}";
            if (closedSet.Contains(key)) continue;
            closedSet.Add(key);

            var successors = IdentifySuccessors(current, width, height, obstacles, targetX, targetY);
            foreach (var successor in successors)
            {
                if (!closedSet.Contains($"{successor.X},{successor.Y}"))
                {
                    openSet.Enqueue(successor, successor.F);
                }
            }
        }

        return new List<(byte dir, byte steps, int x, int y)>();
    }

    private static List<Node> IdentifySuccessors(Node node, int width, int height, byte[] obstacles, int targetX, int targetY)
    {
        var successors = new List<Node>();
        var neighbors = GetPrunedNeighbors(node, width, height, obstacles);

        foreach (var neighbor in neighbors)
        {
            var jumpPoint = Jump(neighbor.X, neighbor.Y, node.X, node.Y, width, height, obstacles, targetX, targetY);
            if (jumpPoint != null)
            {
                jumpPoint.Parent = node;
                jumpPoint.G = node.G + GetDistance(node.X, node.Y, jumpPoint.X, jumpPoint.Y);
                jumpPoint.H = Math.Abs(targetX - jumpPoint.X) + Math.Abs(targetY - jumpPoint.Y);
                jumpPoint.Direction = GetDirectionFromDelta(jumpPoint.X - node.X, jumpPoint.Y - node.Y);
                jumpPoint.StepSize = 1;
                successors.Add(jumpPoint);
            }
        }

        return successors;
    }

    private static List<Node> GetPrunedNeighbors(Node node, int width, int height, byte[] obstacles)
    {
        var neighbors = new List<Node>();
        int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
        int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

        for (int i = 0; i < 8; i++)
        {
            int newX = node.X + dx[i];
            int newY = node.Y + dy[i];

            if (newX < 0 || newX >= width || newY < 0 || newY >= height) continue;
            if (obstacles[newY * width + newX] == 1) continue;

            // 对角线移动时需要检查两个相邻格子是否可通行
            if (i % 2 == 1) // 对角线方向
            {
                int x1 = node.X + dx[i - 1];
                int y1 = node.Y + dy[i - 1];
                int x2 = node.X + dx[(i + 1) % 8];
                int y2 = node.Y + dy[(i + 1) % 8];

                if (obstacles[y1 * width + x1] == 1 || obstacles[y2 * width + x2] == 1)
                    continue;
            }

            neighbors.Add(new Node(newX, newY));
        }

        return neighbors;
    }

    private static Node? Jump(int x, int y, int px, int py, int width, int height, byte[] obstacles, int targetX, int targetY)
    {
        if (x < 0 || x >= width || y < 0 || y >= height || obstacles[y * width + x] == 1)
            return null;

        if (x == targetX && y == targetY)
            return new Node(x, y);

        int dx = x - px;
        int dy = y - py;

        // 对角线移动
        if (dx != 0 && dy != 0)
        {
            // 检查强制邻居
            if ((IsWalkable(x - dx, y + dy, width, height, obstacles) && !IsWalkable(x - dx, y, width, height, obstacles)) ||
                (IsWalkable(x + dx, y - dy, width, height, obstacles) && !IsWalkable(x, y - dy, width, height, obstacles)))
            {
                return new Node(x, y);
            }

            // 递归检查水平和垂直方向
            if (Jump(x + dx, y, x, y, width, height, obstacles, targetX, targetY) != null ||
                Jump(x, y + dy, x, y, width, height, obstacles, targetX, targetY) != null)
            {
                return new Node(x, y);
            }
        }
        else // 直线移动
        {
            if (dx != 0) // 水平移动
            {
                // 检查强制邻居
                if ((IsWalkable(x + dx, y + 1, width, height, obstacles) && !IsWalkable(x, y + 1, width, height, obstacles)) ||
                    (IsWalkable(x + dx, y - 1, width, height, obstacles) && !IsWalkable(x, y - 1, width, height, obstacles)))
                {
                    return new Node(x, y);
                }
            }
            else // 垂直移动
            {
                // 检查强制邻居
                if ((IsWalkable(x + 1, y + dy, width, height, obstacles) && !IsWalkable(x + 1, y, width, height, obstacles)) ||
                    (IsWalkable(x - 1, y + dy, width, height, obstacles) && !IsWalkable(x - 1, y, width, height, obstacles)))
                {
                    return new Node(x, y);
                }
            }
        }

        // 如果没有强制邻居，继续在同一方向跳跃
        return Jump(x + dx, y + dy, x, y, width, height, obstacles, targetX, targetY);
    }

    private static bool IsWalkable(int x, int y, int width, int height, byte[] obstacles)
    {
        return x >= 0 && x < width && y >= 0 && y < height && obstacles[y * width + x] != 1;
    }

    private static int GetDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
    }

    private static List<(byte dir, byte steps, int x, int y)> ReconstructPath(Node node)
    {
        var path = new List<(byte dir, byte steps, int x, int y)>();
        var current = node;

        while (current.Parent != null)
        {
            path.Add((current.Direction, current.StepSize, current.X, current.Y));
            current = current.Parent;
        }

        path.Reverse();
        return path;
    }

    private static List<(byte dir, byte steps, int x, int y)> OptimizePath(List<(byte dir, byte steps, int x, int y)> path)
    {
        if (path.Count < 2) return path;

        var optimizedPath = new List<(byte dir, byte steps, int x, int y)>();
        int i = 0;

        while (i < path.Count)
        {
            // 当前点
            var current = path[i];
            
            // 检查是否可以和下一个点合并
            if (i + 1 < path.Count)
            {
                var next = path[i + 1];
                
                // 如果方向相同且都是步长1，可以合并
                if (current.dir == next.dir && current.steps == 1 && next.steps == 1)
                {
                    // 合并为步长2的点，使用第二个点的坐标
                    optimizedPath.Add((current.dir, 2, next.x, next.y));
                    i += 2; // 跳过下一个点
                    continue;
                }
            }
            
            // 不能合并，保持原样添加
            optimizedPath.Add(current);
            i++;
        }

        return optimizedPath;
    }


    public static int[][] GetMonsPos(MirGameInstanceModel GameInstance)
    {
        var monsters = GameInstance!.Monsters.Where(o => !o.Value.isDead);
        int[][] monsPos = new int[monsters.Count()][];
        int index = 0;
        foreach (var monster in monsters)
        {
            monsPos[index++] = new int[] {
                monster.Value.X,
                monster.Value.Y
            };
        }
        return monsPos;
    }

    public static async Task<bool> PerformPathfinding(CancellationToken cancellationToken, MirGameInstanceModel GameInstance, int tx, int ty, string replaceMap = "",
          int blurRange = 0,
          bool nearBlur = true
        )
    {
        CharacterStatusFunction.GetInfo(GameInstance!);
        MonsterFunction.ReadMonster(GameInstance!);

        var stopwatchTotal = new System.Diagnostics.Stopwatch();
        stopwatchTotal.Start();

        var monsPos = GetMonsPos(GameInstance!);
        var goNodes = genGoPath(GameInstance!, tx, ty, monsPos, blurRange, nearBlur);
        stopwatchTotal.Stop();
        Log.Debug($"寻路: {stopwatchTotal.ElapsedMilliseconds} 毫秒");
        if (goNodes.Count == 0) {
            Log.Warning($"寻路最终未找到");
            return false;
        }

        // 计算每个节点的实际坐标
        var nodePositions = new List<(int x, int y)>();
        int calcX = GameInstance!.CharacterStatus!.X;
        int calcY = GameInstance!.CharacterStatus!.Y;
        foreach (var node in goNodes)
        {
            var (nextX, nextY) = getNextPostion(calcX, calcY, node.dir, node.steps);
            nodePositions.Add((nextX, nextY));
            calcX = nextX;
            calcY = nextY;
        }

        while (goNodes.Count > 0)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            var node = goNodes[0];
            var oldX = GameInstance!.CharacterStatus!.X;
            var oldY = GameInstance!.CharacterStatus!.Y;
            var (nextX, nextY) = getNextPostion(oldX, oldY, node.dir, node.steps);

            GoRunAlgorithm(GameInstance, oldX, oldY, node.dir, node.steps);

            var tried = 0;
            while (true)
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
                if (tried > 6)
                {
                    // 如果在模糊范围内也算成功
                    if (Math.Abs(tx - newX) <= blurRange && Math.Abs(ty - newY) <= blurRange)
                    {
                        return true;
                    }
                    // 尝试跳到后面的点
                    var isJumpSuccess = false;
                    foreach (var jumpSteps in new[] { 1, 2, 3, 4, 5, 10, 11, 15 })
                    {
                        await Task.Delay(100, cancellationToken);
                        if (goNodes.Count <= jumpSteps) continue;

                        // 获取跳跃目标点的坐标
                        var jumpPos = nodePositions[jumpSteps];
                        MonsterFunction.ReadMonster(GameInstance!);
                        monsPos = GetMonsPos(GameInstance!);
                        var jumpPath = genGoPath(GameInstance!, jumpPos.x, jumpPos.y, monsPos);

                        if (jumpPath.Count > 0)
                        {
                            Log.Debug($"尝试跳过{jumpSteps}步，寻路到({jumpPos.x},{jumpPos.y})");

                            // 先走到跳跃点
                            foreach (var pathNode in jumpPath)
                            {
                                var (targetX, targetY) = getNextPostion(GameInstance.CharacterStatus.X, GameInstance.CharacterStatus.Y, pathNode.dir, pathNode.steps);
                                GoRunAlgorithm(GameInstance, GameInstance.CharacterStatus.X, GameInstance.CharacterStatus.Y, pathNode.dir, pathNode.steps);
                                var localTried = 0;
                                while (localTried < 6)
                                {
                                    await Task.Delay(100, cancellationToken);
                                    CharacterStatusFunction.FastUpdateXY(GameInstance!);

                                    // 检查是否成功移动到目标位置
                                    if (GameInstance.CharacterStatus.X == targetX && GameInstance.CharacterStatus.Y == targetY)
                                    {
                                        isJumpSuccess = true;
                                        break;
                                    }
                                    localTried++;
                                }
                                if (localTried > 6)
                                {
                                    isJumpSuccess = false;
                                    break;
                                }
                            }

                            if (isJumpSuccess)
                            {
                                // 移除跳过的路径点和对应的位置信息
                                goNodes.RemoveRange(0, jumpSteps + 1);
                                nodePositions.RemoveRange(0, jumpSteps + 1);
                                Log.Debug($"成功跳过{jumpSteps}步");
                                tried = 0;
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                   
                    }
                    if (!isJumpSuccess)
                    {
                        // 失败了怎么办, 只能放弃先了
                        Log.Warning($"寻路最终未找到 -- 跳点");
                        return false;
                    }
                    else
                    {
                        // 跳出当前点, 并前进了N点, 但是用重装来恢复比较简单
                        return await PerformPathfinding(cancellationToken, GameInstance, tx, ty);
                    }
                }

                if (oldX != newX || oldY != newY)
                {
                    if (nextX == newX && nextY == newY)
                    {
                        goNodes.RemoveAt(0);
                        nodePositions.RemoveAt(0);
                        break;
                    }
                    else
                    {
                        tried = 7; // 触发跳跃尝试
                        continue;
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


