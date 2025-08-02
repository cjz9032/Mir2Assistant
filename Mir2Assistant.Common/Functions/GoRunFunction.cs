
using Mir2Assistant.Common.Models;
using Serilog; // 新增Serilog引用
using System.Diagnostics; // 新增Stopwatch引用
using Mir2Assistant.Common.Utils;
using Mir2Assistant.Common.Constants;
using Mir2Assistant.Common.Services;
using Mir2Assistant.Common.Models.MapPathFinding;
using Mir2Assistant.Common.Generated; // 新增Generated命名空间引用

namespace Mir2Assistant.Common.Functions;
/// <summary>
/// 走路、跑路、寻路
/// </summary>
public static class GoRunFunction
{
    public static MapConnectionService mapConnectionService = new MapConnectionService();

    public static (int x, int y) getNextPostion(int x, int y, byte dir, byte steps)
    {

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
        // gameInstance.GameDebug("执行寻路算法移动，目标: ({X}, {Y}), 方向: {Dir}, 步数: {Steps}", x, y, dir, steps);
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

        SendMirCall.Send(gameInstance, 1001, new nint[] { nextX, nextY, dir, typePara, GameState.MirConfig["角色基址"], GameState.MirConfig["UpdateMsg"] });
    }

    public static (int width, int height, byte[] obstacles) retriveMapObstacles(MirGameInstanceModel gameInstance)
    {
        var id = gameInstance!.CharacterStatus!.MapId;
        if (!gameInstance.MapObstacles.TryGetValue(id, out var data))
        {
            // 读文件获取
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "config/server-define/unity-config/mapc-out", id + ".mapc");
            // binary
            var bytes = File.ReadAllBytes(configPath);
            gameInstance.MapObstacles[id] = bytes;
            data = gameInstance.MapObstacles[id];
        }

        var obstacles = new byte[data.Length - 8];
        Array.Copy(data, 8, obstacles, 0, obstacles.Length);
        // obstacles 前2个int 32是宽高
        var width = BitConverter.ToInt32(data, 0);
        var height = BitConverter.ToInt32(data, 4);
        return (width, height, obstacles);
    }

    public static List<(byte dir, byte steps, int x, int y)> genGoPath(MirGameInstanceModel gameInstance, int targetX, int targetY,
    int blurRange = 0,
    bool nearBlur = false
    )
    {
        var sw = Stopwatch.StartNew();
        var monsPos = GetMonsPos(gameInstance!);
        var (width, height, data) = retriveMapObstacles(gameInstance!);
        int myX = gameInstance!.CharacterStatus!.X;
        int myY = gameInstance!.CharacterStatus!.Y;
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
        if (blurRange > 0)
        {
            List<(int X, int Y)> candidatePoints = new List<(int X, int Y)>();

            // 以target为中心，在range范围内选取n*n的范围
            for (int y = targetY - blurRange; y <= targetY + blurRange; y++)
            {
                for (int x = targetX - blurRange; x <= targetX + blurRange; x++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height && data[y * width + x] != 1)
                    {
                        // 计算到起点和终点的距离
                        double distToStart = Math.Sqrt(Math.Pow(x - myX, 2) + Math.Pow(y - myY, 2));
                        double distToTarget = Math.Sqrt(Math.Pow(x - targetX, 2) + Math.Pow(y - targetY, 2));
                        // 综合距离评分：起点距离权重0.7，终点距离权重0.3
                        //double score = nearBlur ?
                        //    (99 * distToStart + 0.3 * distToTarget) :  // 近距离优先
                        //    (0.3 * distToStart + 99 * distToTarget);   // 远距离优先

                        candidatePoints.Add((x, y));
                    }
                }
            }

            // 根据综合距离评分排序
            //candidatePoints = candidatePoints.OrderBy(p => p.Distance).ToList();

            targetX = -1;
            targetY = -1;

            if (candidatePoints.Count > 0)
            {
                // 从前3个点中随机选择一个（如果不足3个则在现有点中随机选择）
                var random = new Random();
                //var topPoints = candidatePoints.Take(Math.Min(5, candidatePoints.Count)).ToList();
                var selectedPoint = candidatePoints[random.Next(candidatePoints.Count)];
                targetX = selectedPoint.X;
                targetY = selectedPoint.Y;
                gameInstance.GameDebug($"从{candidatePoints.Count}个最佳点中随机选择目标点: ({targetX}, {targetY})");
            }

            if (targetX == -1)
            {
                sw.Stop();
                gameInstance.GameDebug($"无法找到有效的模糊目标点，耗时: {sw.ElapsedMilliseconds}ms");
                return new List<(byte dir, byte steps, int x, int y)>();
            }
        }
        else if (data[targetY * width + targetX] == 1)
        {
            sw.Stop();
            gameInstance.GameDebug($"目标点不可达，耗时: {sw.ElapsedMilliseconds}ms");
            return new List<(byte dir, byte steps, int x, int y)>();
        }

        // 分距离, 100以内直接a星
        List<(byte dir, byte steps, int x, int y)> path = new List<(byte dir, byte steps, int x, int y)>();
        path = FindPathCoreSmallWithAStar(width, height, data, myX, myY, targetX, targetY);
        // 优化路径 TODO 暂时先不用 费血
        path = OptimizePath(path);
        sw.Stop();
        gameInstance.GameDebug($"寻路完成: 起点({myX},{myY}) -> 终点({targetX},{targetY}), 路径长度: {path.Count}, 总耗时(含数据准备): {sw.ElapsedMilliseconds}ms");
        return path.Select(p => (p.dir, p.steps, p.x, p.y)).ToList();
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
            var current = path[i];

            // 检查是否可以和下一个点合并
            if (i + 1 < path.Count && current.steps == 1)
            {
                var next = path[i + 1];
                bool canMerge = current.dir == next.dir &&
                               next.steps == 1 &&
                               IsConsecutivePoints(current.x, current.y, next.x, next.y, current.dir);

                if (canMerge)
                {
                    var merged = (current.dir, (byte)2, next.x, next.y);
                    optimizedPath.Add(merged);
                    i += 2;
                    continue;
                }
            }
            optimizedPath.Add(current);
            i++;
        }

        return optimizedPath;
    }

    // 检查两个点是否是连续的（基于方向）
    private static bool IsConsecutivePoints(int x1, int y1, int x2, int y2, byte dir)
    {
        // 根据方向计算期望的下一个点的坐标
        var (expectedX, expectedY) = getNextPostion(x1, y1, dir, 1);
        return expectedX == x2 && expectedY == y2;
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
    
    public static (int, int)[] GenMobCleanPairs(MirGameInstanceModel instanceValue, string mapId){
        var CharacterStatus = instanceValue.CharacterStatus!;
        var patrolSteps = 10;
        var portalStartX = 0;
        var portalEndX = 0;
        var portalStartY = 0;
        var portalEndY = 0;
        var fixedPoints = new List<(int, int)>();
        if (mapId == "0")
        {
            // 地图优化
            var isLeftAlive = CharacterStatus.X < 400;
            portalStartX = isLeftAlive ? 200 : 550;
            portalEndX = isLeftAlive ? 300 : 620;
            portalStartY = 550;
            portalEndY = 620;
            if (CharacterStatus.Level > 13)
            {
                portalStartX = 50;
                portalEndX = 250;
                portalStartY = 350;
                portalEndY = 550;
            }


            // 生成矩形区域内的所有点位
            for (int x = portalStartX; x <= portalEndX; x += patrolSteps)
            {
                // 根据x的奇偶性决定y的遍历方向，形成蛇形路线
                var yStart = (x - portalStartX) / patrolSteps % 2 == 0 ? portalStartY : portalEndY;
                var yEnd = (x - portalStartX) / patrolSteps % 2 == 0 ? portalEndY : portalStartY;
                var yStep = (x - portalStartX) / patrolSteps % 2 == 0 ? patrolSteps : -patrolSteps;

                for (int y = yStart; yStep > 0 ? y <= yEnd : y >= yEnd; y += yStep)
                {
                    // 生成点位
                    fixedPoints.Add((x, y));
                    // Log.Debug($"生成巡逻点: ({x}, {y})");
                }
            }
        }
        else
        {
            // 获取该地图数据
            // var (width, height, obstacles) = retriveMapObstacles(instanceValue!);
            var hangPoints = HangPointData.GetHangPoints(mapId);
            foreach (var point in hangPoints)
            {
                fixedPoints.Add((point[0], point[1]));
            }
        }
      

        Log.Information($"共生成 地图 {CharacterStatus.MapId} 的 {fixedPoints.Count} 个固定巡逻点 from {portalStartX} to {portalEndX} from {portalStartY} to {portalEndY}");

        // 转换为数组
        return fixedPoints.ToArray();
    }

    public static async Task<bool> NormalAttackPoints(MirGameInstanceModel instanceValue, CancellationToken _cancellationToken, bool forceSkip, Func<MirGameInstanceModel, bool> checker, string mapId = "")
    {
    
        if (instanceValue.CharacterStatus!.CurrentHP == 0)
        {
            instanceValue.GameWarning("角色已死亡，无法执行巡逻攻击");
            return false;
        }
        if (mapId == "") mapId = instanceValue.CharacterStatus.MapId;
        var patrolPairs = new (int, int)[] { (0, 0) };
        if (!forceSkip)
        {
            patrolPairs = GenMobCleanPairs(instanceValue, mapId);
        }

        instanceValue.GameDebug("开始巡逻攻击，巡逻点数量: {Count}", patrolPairs.Length);
        var allowMonsters = new string[]  {"鸡", "鹿", "羊", "食人花","稻草人", "多钩猫", "钉耙猫", "半兽人", "半兽战士", "半兽勇士",
                "森林雪人", "蛤蟆", "蝎子",
                "毒蜘蛛", "洞蛆", "蝙蝠", "骷髅","骷髅战将", "掷斧骷髅", "骷髅战士", "僵尸","山洞蝙蝠"};
        // 等级高了不打鸡鹿
        if (instanceValue.CharacterStatus!.Level > 10)
        {
            allowMonsters = allowMonsters.Skip(2).ToArray();
        }
        var allowButch = new string[] { "鹿", "羊" }; // 不要 "鸡", "毒蜘蛛", "蝎子", "洞蛆",
        // 当前巡回
        var curP = 0;
        var CharacterStatus = instanceValue.CharacterStatus!;
        if (!forceSkip)
        {
            // 查找离我最近的巡逻点
            curP = patrolPairs
                .Select((p, i) => (i, dis: Math.Max(Math.Abs(p.Item1 - CharacterStatus.X), Math.Abs(p.Item2 - CharacterStatus.Y))))
                .MinBy(x => x.dis)
                .i;
        }
        // 巡逻太多次了 有问题
        var patrolTried = 0;
        while (true)
        {
            instanceValue.GameDebug("开始巡逻攻击，巡逻点 {CurP}", curP);
            await Task.Delay(100);
            patrolTried++;
            if (patrolTried > 200)
            {
                instanceValue.GameWarning("巡逻攻击失败，巡逻点 {CurP}", curP);
                return false;
            }
            // 不寻路模式, 其实就是只打怪, 需要抽象

            // 主从模式
            // 主人是点位
            var (px, py) = (0, 0);
            if (!forceSkip)
            {
                if (checker(instanceValue!))
                {
                    break;
                }
                // 从是跟随
                if (instanceValue.AccountInfo.IsMainControl)
                {
                    // 主人是点位
                    (px, py) = patrolPairs[curP];
                    bool _whateverPathFound = await PerformPathfinding(_cancellationToken, instanceValue!, px, py, mapId, 5, true, 10);
                }
                else
                {
                    // 从是跟随
                    var mainInstance = GameState.GameInstances[0];
                    if (mainInstance.IsAttached)
                    {
                        (px, py) = (mainInstance.CharacterStatus!.X!, mainInstance.CharacterStatus!.Y!);
                    }
                    bool _whateverPathFound = await PerformPathfinding(_cancellationToken, instanceValue!, px, py, mainInstance.CharacterStatus.MapId, 5, true, 10);
                }
            }


            // 如果是跟随
            if (!instanceValue.AccountInfo!.IsMainControl && !forceSkip)
            {
                // 从是跟随 -- 这是重复代码 先放着
                var mainInstance = GameState.GameInstances[0];
                if (mainInstance.IsAttached)
                {
                    (px, py) = (mainInstance.CharacterStatus!.X!, mainInstance.CharacterStatus!.Y!);
                }
                // 检测距离
                if (Math.Max(Math.Abs(px - CharacterStatus.X), Math.Abs(py - CharacterStatus.Y)) > 12)
                {
                    // 跟随
                    Log.Information("跟随 in start: {X}, {Y}", px, py);
                    await PerformPathfinding(_cancellationToken, instanceValue!, px, py, mainInstance.CharacterStatus.MapId, 3, true, 10);
                }
            }

            var monsterTried = 0;
            // 无怪退出
            var firstMonPos = (0, 0);
            while (true)
            {
                await Task.Delay(100);
                monsterTried++;
                if (monsterTried > 100)
                {
                    instanceValue.GameWarning("怪物攻击失败，巡逻点 {CurP}", curP);
                    return false;
                }
                // 发现活人先停下 并且不是自己人
                var zijiren = GameState.GameInstances.Select(o => o.CharacterStatus.Name);
                var otherPeople = instanceValue.Monsters.Values.Where(o => o.TypeStr == "玩家" && !zijiren.Contains(o.Name)).FirstOrDefault();
                var huorend = 0;
                if (otherPeople != null && otherPeople.CurrentHP > 0)
                {
                    instanceValue.GameInfo($"发现活人{otherPeople.Name}  hp {otherPeople.CurrentHP} level {otherPeople.Level} 停下");
                    await Task.Delay(1000);
                    huorend++;
                    if (huorend > 15)
                    {
                        break;
                    }
                    continue;
                }
                // todo 法师暂时不要砍了 要配合2边一起改
                if (instanceValue.AccountInfo.role == RoleType.mage && instanceValue.CharacterStatus!.Level < 11)
                {
                    await Task.Delay(100);
                    break;
                }
                // 检测距离
                if (!instanceValue.AccountInfo.IsMainControl && !forceSkip)
                {
                    var mainInstance = GameState.GameInstances[0];
                    if (mainInstance.IsAttached)
                    {
                        (px, py) = (mainInstance.CharacterStatus!.X!, mainInstance.CharacterStatus!.Y!);
                    }
                    if (Math.Max(Math.Abs(px - CharacterStatus.X), Math.Abs(py - CharacterStatus.Y)) > 12)
                    {
                        Log.Information("跟随 in monster: {X}, {Y}", px, py);
                        await PerformPathfinding(_cancellationToken, instanceValue!, px, py, mainInstance.CharacterStatus.MapId, 3, true, 10);
                    }
                }
                // 查看存活怪物 并且小于距离10个格子
                var ani = instanceValue.Monsters.Values.Where(o => o.stdAliveMon &&
                // 暂时取消
                // !instanceValue.attackedMonsterIds.Contains(o.Id) &&
                allowMonsters.Contains(o.Name) &&
                // 还要看下是不是距离巡逻太远了, 就不要, 
                (firstMonPos.Item1 == 0 ? true : Math.Max(Math.Abs(o.X - firstMonPos.Item1), Math.Abs(o.Y - firstMonPos.Item2)) < 16)
                 && Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)) < 13)
                // 还要把鹿羊鸡放最后
                .OrderBy(o => o.Name == "鹿" || o.Name == "羊" || o.Name == "鸡" ? 1 : 0)
                .ThenBy(o => measureGenGoPath(instanceValue!, o.X, o.Y))
                .FirstOrDefault();
                if (ani != null)
                {
                    if (firstMonPos.Item1 == 0)
                    {
                        firstMonPos = (ani.X, ani.Y);
                    }
                    instanceValue.GameDebug("发现目标怪物: {Name}, 位置: ({X}, {Y}), 距离: {Distance}",
                ani.Name, ani.X, ani.Y,
                Math.Max(Math.Abs(ani.X - CharacterStatus.X), Math.Abs(ani.Y - CharacterStatus.Y)));
                    // 持续攻击, 超过就先放弃
                    var monTried = 0;
                    // 等待初始到怪面前的时间 根据初始距离推算 200ms 一格, 保持loop delay一致
                    var INIT_WAIT = Math.Max(Math.Abs(ani.X - CharacterStatus.X), Math.Abs(ani.Y - CharacterStatus.Y));
                    while (true)
                    {
                        monTried++;
                        MonsterFunction.SlayingMonster(instanceValue!, ani.Addr);
                        // 这时候可能找不到了就上去, 或者是会跑的少数不用管
                        if (monTried > INIT_WAIT && Math.Max(Math.Abs(ani.X - CharacterStatus.X), Math.Abs(ani.Y - CharacterStatus.Y)) > 1)
                        {
                            MonsterFunction.SlayingMonsterCancel(instanceValue!);
                            await PerformPathfinding(_cancellationToken, instanceValue!, ani.X, ani.Y, "", 1, true, 999);
                            instanceValue.Monsters.TryGetValue(ani.Id, out MonsterModel? ani3);
                            if (ani3 == null)
                            {
                                break;
                            }
                            ani = ani3;
                        }
                        await Task.Delay(200);
                        // todo 优化
                        instanceValue.Monsters.TryGetValue(ani.Id, out MonsterModel? ani2);
                        if (ani2 == null)
                        {
                            break;
                        }
                        ani = ani2;
                        if (ani.isDead || monTried > 150)
                        {
                            instanceValue.attackedMonsterIds.Add(ani.Id);
                            MonsterFunction.SlayingMonsterCancel(instanceValue!);
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
                // if (px == 0 && py == 0)
                // {

                // }
                if (checker(instanceValue!))
                {
                    break;
                }
            }


            var miscs = instanceValue.Items.Where(o => !o.IsEmpty);
            var megaCount = miscs.Count(o => GameConstants.Items.MegaPotions.Contains(o.Name));
            var healCount = miscs.Count(o => GameConstants.Items.HealPotions.Contains(o.Name));
            // 没怪了 可以捡取东西 或者挖肉
            // 捡取
            // 按距离, 且没捡取过
            var drops = instanceValue.DropsItems.Where(o => o.Value.IsGodly || (!instanceValue.pickupItemIds.Contains(o.Value.Id)
            && !GameConstants.Items.binItems.Contains(o.Value.Name)
            // 药
            && (!(GameConstants.Items.HealPotions.Contains(o.Value.Name) && healCount > 6))
            && (GameConstants.Items.MegaPotions.Contains(o.Value.Name) ? (
                    instanceValue.AccountInfo.role == RoleType.blade ? (CharacterStatus.Level > 28 && megaCount < 6)
                    : true
                ) : true)
            ))
            .OrderBy(o => measureGenGoPath(instanceValue!, o.Value.X, o.Value.Y));
            foreach (var drop in drops)
            {
                instanceValue.GameDebug("准备拾取物品，位置: ({X}, {Y})", drop.Value.X, drop.Value.Y);
                bool pathFound2 = await PerformPathfinding(_cancellationToken, instanceValue!, drop.Value.X, drop.Value.Y, "", 0, true, 5);
                if (pathFound2)
                {
                    var miscs2 = instanceValue.Items.Where(o => !o.IsEmpty);
                    // 极品满就扔东西 -- todo 还有 自定义极品
                    if (drop.Value.IsGodly && miscs2.Count() == 40)
                    {
                        // 扔东西
                        // 挑选一个扔, 一般扔药
                        var needDropItem = miscs2.FirstOrDefault(o => GameConstants.Items.HealPotions.Contains(o.Name) ||
                            GameConstants.Items.MegaPotions.Contains(o.Name)
                        );
                        if (needDropItem != null)
                        {
                            NpcFunction.EatIndexItem(instanceValue!, needDropItem.Index, true);
                            await Task.Delay(200);
                        }
                        // 否则就随便找个装备 同样要排除自定义极品
                        // if (needDropItem != null)
                        // {
                        //     // await ItemFunction.Drop(instanceValue!, needDropItem.Index);
                        // }
                    }

                    ItemFunction.Pickup(instanceValue!);
                    // 加捡取过的名单,
                    instanceValue.pickupItemIds.Add(drop.Value.Id);
                }
            }

            // 屠挖肉
            if (miscs.Count() < 32)
            {
                var bodys = instanceValue.Monsters.Values.Where(o => o.isDead && allowButch.Contains(o.Name) && !o.isButched && Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)) < 13)
               .OrderBy(o => Math.Max(Math.Abs(o.X - CharacterStatus.X), Math.Abs(o.Y - CharacterStatus.Y)));
                foreach (var body in bodys)
                {
                    instanceValue.GameDebug("准备屠宰: {Name}, 位置: ({X}, {Y})", body.Name, body.X, body.Y);
                    bool pathFound2 = await PerformPathfinding(_cancellationToken, instanceValue!, body.X, body.Y, "", 2, true, 1);
                    if (pathFound2)
                    {
                        // 要持续屠宰, 直到尸体消失, 最大尝试 30次
                        var tried = 0;
                        while (tried < 20)
                        {
                            SendMirCall.Send(instanceValue!, 3030, new nint[] { (nint)body.X, (nint)body.Y, 0, body.Id });
                            await Task.Delay(500);
                            MonsterFunction.ReadMonster(instanceValue!);
                            if (body.isButched)
                            {
                                break;
                            }
                            tried++;
                        }
                    }
                }
            }

            // checker 满足条件就跳出循环, checker是参数
            if (checker(instanceValue!))
            {
                break;
            }
            curP++;
            curP = curP % patrolPairs.Length;
            continue;
        }
        return true;

    }
    
    public static int measureGenGoPath (MirGameInstanceModel GameInstance, int tx, int ty){
        try
        {
            // 身边不用寻
            int myX = GameInstance!.CharacterStatus!.X;
            int myY = GameInstance!.CharacterStatus!.Y;
            if(Math.Abs(tx - myX) < 2 && Math.Abs(ty - myY) < 2){
                return 0;
            }
            var monsPos = GetMonsPos(GameInstance!);
            var res = genGoPath(GameInstance!, tx, ty, 1, true).Count();
            return res == 0 ? 999 : res;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "寻路测距异常");
        }
        return 999;
    }

    public static async Task cleanMobs(MirGameInstanceModel GameInstance, int attacksThan, CancellationToken cancellationToken) {
        // todo 法师暂时不要砍了 要配合2边一起改
        if (attacksThan > 0 && GameInstance.AccountInfo.role != RoleType.mage)
        {
            var temp = new string []{ "鸡", "鹿", "羊"};
            // 攻击怪物, 太多了 过不去
            var monsters = GameInstance.Monsters.Where(o => o.Value.stdAliveMon && !temp.Contains(o.Value.Name)).ToList();
            if (monsters.Count > attacksThan)
            {
                await NormalAttackPoints(GameInstance, cancellationToken, true, (instanceValue) =>
                {
                    // 重读怪物
                    var existsCount = instanceValue.Monsters.Where(o => o.Value.stdAliveMon && !temp.Contains(o.Value.Name)).Count();
                    // 怪物死了剩余一半就可以通过
                    if (existsCount <= monsters.Count / 2)
                    {
                        return true;
                    }
                    return false;
                });
            }
        }
    }

    public static async Task<bool> PerformPathfinding(CancellationToken cancellationToken, MirGameInstanceModel GameInstance, int tx, int ty, string replaceMap = "",
          int blurRange = 0,
          bool nearBlur = true,
          int attacksThan = 3,
          int retries = 0
        )
    {
        if (cancellationToken.IsCancellationRequested)
        {
            GameInstance.GameDebug("寻路被取消");
            return false;
        }
        if (GameInstance.CharacterStatus!.CurrentHP == 0)
        {
            GameInstance.GameWarning("角色已死亡，无法执行寻路");
            await Task.Delay(5_000);
            return false;
        }
        CharacterStatusFunction.GetInfo(GameInstance!);
        MonsterFunction.ReadMonster(GameInstance!);
        // 起点等于终点 结束
        if (GameInstance.CharacterStatus.X == tx && GameInstance.CharacterStatus.Y == ty && replaceMap == "")
        {
            return true;
        }
        replaceMap = replaceMap == "" ? GameInstance.CharacterStatus.MapId : replaceMap;
        // 支持跨多图寻路 返回值要改数组,并且后续数组都是先占位
        var connectionsPath = new List<MapConnection>();
        var isAcross = replaceMap != GameInstance.CharacterStatus.MapId;
        if (isAcross)
        {
            // 先占位
            connectionsPath = mapConnectionService.FindPath(GameInstance.CharacterStatus.MapId, replaceMap);
            if (connectionsPath == null)
            {
                return false;
            }
        }
        else
        {
            connectionsPath = new List<MapConnection>()
            {
                new MapConnection()
                {
                    From = new MapPosition()
                    {
                        // 有点tricky 懒得改类型了
                        X = tx, Y = ty,
                        MapId = GameInstance.CharacterStatus.MapId,
                    },
                    To = new MapPosition()
                    {
                        X = 999,
                        Y = 999,
                        MapId = replaceMap,
                    }
                }
            };
        }
        // 以下就变for循环结构了, 因为要1->N个地图寻路, 全部可以泛化成A->B
        for (var i = 0; i < connectionsPath.Count; i++)
        {
            var connection = connectionsPath[i];
            // 检查当前是否所在地图 否则说明失效 重新搞
            if (GameInstance.CharacterStatus.MapId != connection.From.MapId)
            {
                // reset
                return await PerformPathfinding(cancellationToken, GameInstance, tx, ty, replaceMap, blurRange, nearBlur, attacksThan, retries + 1);
            }

            var stopwatchTotal = new System.Diagnostics.Stopwatch();
            stopwatchTotal.Start();
            var goNodes = new List<(byte dir, byte steps, int x, int y)>();
            try
            {
                goNodes = genGoPath(GameInstance!, connection.From.X, connection.From.Y, blurRange, nearBlur).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "寻路异常");
                await Task.Delay(100);
                if (GameInstance.CharacterStatus!.CurrentHP == 0)
                {
                    await Task.Delay(60_000);
                }
                return false;
            }

            stopwatchTotal.Stop();
            if (stopwatchTotal.ElapsedMilliseconds > 10)
            {
                GameInstance.GameDebug("寻路: {Time} 毫秒", stopwatchTotal.ElapsedMilliseconds);
            }


            if (goNodes.Count == 0)
            {
                await cleanMobs(GameInstance, attacksThan, cancellationToken);
                // 加个重试次数3次
                await Task.Delay(200);

                if (retries < 3)
                {
                    GameInstance.GameWarning("寻路未找到路径，准备第 {Retry} 次重试", retries + 1);
                    return await PerformPathfinding(cancellationToken, GameInstance, tx, ty, replaceMap, blurRange, nearBlur, attacksThan, retries + 1);
                }

                GameInstance.GameWarning("寻路最终未找到路径，已重试 {Retries} 次", retries);
                return false;
            }


            while (goNodes.Count > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
                if (GameInstance.CharacterStatus!.CurrentHP == 0)
                {
                    return false;
                }
                await cleanMobs(GameInstance, attacksThan, cancellationToken);

                var node = goNodes[0];
                var oldX = GameInstance!.CharacterStatus!.X;
                var oldY = GameInstance!.CharacterStatus!.Y;
                var (nextX, nextY) = getNextPostion(oldX, oldY, node.dir, node.steps);

                GoRunAlgorithm(GameInstance, oldX, oldY, node.dir, node.steps);

                var tried = 0;
                var maxed = 9;
                while (true)
                {
                    if (GameInstance.CharacterStatus!.CurrentHP == 0)
                    {
                        return false;
                    }
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
                    if (tried > maxed)
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
                            var jumpPos = goNodes[jumpSteps];
                            MonsterFunction.ReadMonster(GameInstance!);
                            var jumpPath = new List<(byte dir, byte steps, int x, int y)>();
                            try
                            {
                                jumpPath = genGoPath(GameInstance!, jumpPos.x, jumpPos.y).ToList();
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "寻路异常");
                                await Task.Delay(100);
                                if (GameInstance.CharacterStatus!.CurrentHP == 0)
                                {
                                    await Task.Delay(60_000);
                                }
                                return false;
                            }

                            if (jumpPath.Count > 0)
                            {
                                GameInstance.GameDebug($"尝试跳过{jumpSteps}步，寻路到({jumpPos.x},{jumpPos.y})");

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
                                    GameInstance.GameDebug($"成功跳过{jumpSteps}步");
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
                            GameInstance.GameWarning($"寻路最终未找到 -- 跳点 再次尝试 NB");
                            return await PerformPathfinding(cancellationToken, GameInstance, tx, ty, replaceMap, blurRange + 1, nearBlur, attacksThan, retries + 1);
                            // return false;
                        }
                        else
                        {
                            // 跳出当前点, 并前进了N点, 但是用重装来恢复比较简单
                            return await PerformPathfinding(cancellationToken, GameInstance, tx, ty, replaceMap, blurRange, nearBlur, attacksThan, retries + 1);
                        }
                    }

                    if (oldX != newX || oldY != newY)
                    {
                        if (nextX == newX && nextY == newY)
                        {
                            goNodes.RemoveAt(0);
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    // 否则继续等待
                }
            }
            // 多图就需要等待
            if(isAcross)
            {
                await Task.Delay(1000);
            }
        }

        return true;
    }


    public static async Task RestartByToSelectScene(MirGameInstanceModel gameInstance)
    {
        SendMirCall.Send(gameInstance, 9098, new nint[] { });
        await Task.Delay(5000);
        SendMirCall.Send(gameInstance, 9099, new nint[] { });
        await Task.Delay(6000);
        SendMirCall.Send(gameInstance, 9100, new nint[] { });
        await Task.Delay(3000);
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

    public static bool sendSpell(MirGameInstanceModel GameInstance, int spellId, int x = 0, int y = 0, int targetId = 0)
    {
        // check mp
        if (GameInstance.CharacterStatus!.CurrentMP < 10)
        {
            return false;
        }
        if (GameInstance.spellLastTime + 1200 > Environment.TickCount)
        {
            return false;
        }
        GameInstance.spellLastTime = Environment.TickCount;
        // check instance cool time to avoid ban
        SendMirCall.Send(GameInstance, 3100, new nint[] { spellId, x, y, targetId });
        return true;
    }

    public static bool CapbilityOfHeal(MirGameInstanceModel GameInstance)
    {
        // role
        if (GameInstance.AccountInfo.role != RoleType.taoist
        || GameInstance.CharacterStatus!.Level < 7)
        {
            return false;
        }
        return true;
    }

    public static void TryHealPeople(MirGameInstanceModel GameInstance)
    {
        if (!CapbilityOfHeal(GameInstance))
        {
            // GameInstance.GameDebug("角色无法治疗他人，跳过治疗检查");
            return;
        }
        // GameInstance.GameDebug("开始检查需要治疗的目标");
        // pick the needed people
        // 组队成员
        var instances = GameState.GameInstances;

        // 添加别的客户端的怪物信息 -- todo 远程机器信息 开socket连接
        var allMonsInClients = new List<MonsterModel>();
        foreach (var instance in instances)
        {
            // 只取这个实例里对应自己账号的玩家信息和宝宝信息
            var selfMonsters = instance.Monsters.Values.Where(m => 
                (m.TypeStr == "玩家" && m.Name == instance.AccountInfo.CharacterName) ||  // 玩家自己
                (m.TypeStr == "(怪)" && m.Name.Contains(instance.AccountInfo.CharacterName))  // 玩家的宝宝
            );
            allMonsInClients.AddRange(selfMonsters);
        }

        var people = allMonsInClients.Where(o =>
            // not in cd
            !GameInstance.healCD.TryGetValue(o.Id, out var cd) || Environment.TickCount > cd + GameConstants.Skills.HealPeopleCD &&
            // 活着
            o.CurrentHP > 0 &&
            !o.isDead
            // 低血量
            && ((o.CurrentHP < o.MaxHP * 0.7) || o.CurrentHP < 10)
            // 距离足够
            && (Math.Abs(GameInstance.CharacterStatus.X - o.X) < 12
            && Math.Abs(GameInstance.CharacterStatus.Y - o.Y) < 12)
        )
        // 按优先级排序, 人物总是比宝宝优先, 绝对值低血量优先
        .OrderBy(o => o.TypeStr == "玩家" ? 0 : 1)
        .ThenBy(o => Math.Abs(o.CurrentHP - o.MaxHP * 0.7))
        .FirstOrDefault();

        if (people == null)
        {
            // GameInstance.GameDebug("未找到需要治疗的目标");
            return;
        }

        GameInstance.GameInfo("准备治疗目标: {Name}, HP: {HP}/{MaxHP}", people.Name, people.CurrentHP, people.MaxHP);
        sendSpell(GameInstance, GameConstants.Skills.HealSpellId, people.X, people.Y, people.Id);
        GameInstance.healCD[people.Id] = Environment.TickCount;
    }

    public static int[]? findIdxInAllItems(MirGameInstanceModel GameInstance, string name)
    {
        // TODO 打包这种还没算
        var bagItems2 = GameInstance.Items;
        var idx = bagItems2.Where(o => o.Name == name).ToList();
        if (idx.Count > 0)
        {
            return idx.Select(o => o.Index + 6).ToArray();
        }
        else
        {
            var quickItems = GameInstance.QuickItems;
            var idx2 = quickItems.Where(o => o.Name == name).ToList();
            if (idx2.Count > 0)
            {
                return idx2.Select(o => o.Index).ToArray();
            }
        }
        return null;
    }

    public static void TryEatDrug(MirGameInstanceModel GameInstance)
    {
        var hp = GameInstance.CharacterStatus.CurrentHP;
        var maxHp = GameInstance.CharacterStatus.MaxHP;
        var mp = GameInstance.CharacterStatus.CurrentMP;
        var maxMp = GameInstance.CharacterStatus.MaxMP;
        // GameInstance.GameDebug("检查是否需要吃药，当前HP: {HP}/{MaxHP}, MP: {MP}/{MaxMP}", hp, maxHp, mp, maxMp);
        // todo 解包再吃
        //  for low hp
        if (GameInstance.CharacterStatus.CurrentHP < GameInstance.CharacterStatus.MaxHP * 0.4) // 0.5避免浪费治疗
        {

                var veryLow = GameInstance.CharacterStatus.CurrentHP < GameInstance.CharacterStatus.MaxHP * 0.2;
                var items = veryLow ? GameConstants.Items.SuperPotions : GameConstants.Items.HealPotions;
                int resIdx = -1;
                foreach (var item in items)
                {
                    var idx = findIdxInAllItems(GameInstance, item);
                    if (idx != null)
                    {
                        resIdx = idx[0];
                        break;
                    }
                }

                if (resIdx == -1)
                {
                    return;
                }

                NpcFunction.EatIndexItem(GameInstance, resIdx);
        }

        // for low mp
        if (GameInstance.CharacterStatus.CurrentMP < GameInstance.CharacterStatus.MaxMP * 0.6 || GameInstance.CharacterStatus.CurrentMP < 10)
        {
            // 找蓝药 太阳水
            var veryLow = GameInstance.CharacterStatus.CurrentMP < GameInstance.CharacterStatus.MaxMP * 0.2;
            var items = veryLow ? GameConstants.Items.SuperPotions : GameConstants.Items.MegaPotions;
            int resIdx = -1;
            foreach (var item in items)
            {
                var idx = findIdxInAllItems(GameInstance, item);
                if (idx != null)
                {
                    resIdx = idx[0];
                    break;
                }
            }
            if (resIdx == -1)
            {
                return;
            }
            NpcFunction.EatIndexItem(GameInstance, resIdx);
        }
    }
}


