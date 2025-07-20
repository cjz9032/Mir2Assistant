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
using static Mir2Assistant.Common.Utils.WindowUtils;
using static System.Windows.Forms.AxHost;

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
    int[][] monsPos ,
    int blurRange = 0,
    bool nearBlur = false
    )
    {
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
        int myX = gameInstance!.CharacterStatus!.X!.Value;
        int myY = gameInstance!.CharacterStatus!.Y!.Value;
        // 提取后续data
        var data = new byte[obstacles.Length - 8];
        Array.Copy(obstacles, 8, data, 0, data.Length);


        // 添加怪物位置作为障碍点
        // todo 其实是反着的,需要往上并且加容错选项
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
                    return new List<(byte dir, byte steps)>();
                }
            } else {
                return new List<(byte dir, byte steps)>();
            }
        }



        // 执行寻路算法
        return FindPath(width, height, data, myX, myY, targetX, targetY);
    }
     


    // A* 寻路算法实现
    public static List<(byte dir, byte steps)> FindPath(int width, int height, byte[] mapData, int startX, int startY, int endX, int endY)
    {
        // 定义8个方向的偏移量
        var directions = new (int dx, int dy)[8] {
            (0, -1), (1, -1), (1, 0), (1, 1),
            (0, 1), (-1, 1), (-1, 0), (-1, -1)
        };

        Func<int, int, bool> IsValid = (x, y) => x >= 0 && x < width && y >= 0 && y < height;
        // 检查路径是否有障碍物
        // 预计算路径是否畅通的结果，避免重复计算，限制缓存大小
        var pathCache = new Dictionary<(int, int, int, int), bool>();
        const int MaxCacheSize = 1000;
        Func<int, int, int, int, bool> IsPathClear = (startX, startY, endX, endY) => {
            var key = (startX, startY, endX, endY);
            if (pathCache.TryGetValue(key, out bool result))
            {
                return result;
            }
            
            int dx = endX - startX;
            int dy = endY - startY;
            int steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
            for (int i = 1; i <= steps; i++)
            {
                int x = startX + dx * i / steps;
                int y = startY + dy * i / steps;
                if (mapData[y * width + x] == 1)
                {
                    if (pathCache.Count >= MaxCacheSize) pathCache.Clear();
                    pathCache[key] = false;
                    return false;
                }
            }
            if (pathCache.Count >= MaxCacheSize) pathCache.Clear();
            pathCache[key] = true;
            return true;
        };

        // 使用二叉堆优化openSet
        var openSet = new BinaryHeap<Node>();
        // 使用二维数组优化closedSet
        var closedSet = new bool[width, height];
        var startNode = new Node(startX, startY, 0, Heuristic(startX, startY, endX, endY));
        openSet.Insert(startNode);

        while (openSet.Count > 0)
        {
            var current = openSet.ExtractMin();

            if (current.X == endX && current.Y == endY)
            {
                // 重建路径
                var path = new List<(byte dir, byte steps)>();
                while (current.Parent != null)
                {
                    if (current.Action.HasValue)
                    {
                        path.Insert( (byte)0, current.Action.Value );
                    }
                    current = current.Parent;
                }
                return path;
            }

            closedSet[current.X, current.Y] = true;

            // 尝试8个方向，优先尝试步长为2，若不通则尝试步长为1
            const int MaxNodes = 10000; // 最大节点数限制
            for (byte dir = 0; dir < 8; dir++)
            {
                if (openSet.Count >= MaxNodes) break;
                // 尝试步长为2
                int newX = current.X + directions[dir].dx * 2;
                int newY = current.Y + directions[dir].dy * 2;

                if (IsValid(newX, newY) && !closedSet[newX, newY] && IsPathClear(current.X, current.Y, newX, newY))
                {
                    int newG = current.G + 20;
                    var neighbor = new Node(newX, newY, newG, Heuristic(newX, newY, endX, endY), current, (dir, (byte)2));
                    openSet.Insert(neighbor);
                }
                else
                {
                    // 步长为2不通，尝试步长为1
                    newX = current.X + directions[dir].dx;
                    newY = current.Y + directions[dir].dy;
                    if (IsValid(newX, newY) && !closedSet[newX, newY] && IsPathClear(current.X, current.Y, newX, newY))
                    {
                        int newG = current.G + 10;
                        var neighbor = new Node(newX, newY, newG, Heuristic(newX, newY, endX, endY), current, (dir, (byte)1));
                        openSet.Insert(neighbor);
                    }
                }
            }
        }

        return new List<(byte dir, byte steps)>();


    }
    // 节点类
    private class Node : IComparable<Node>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int G { get; set; }
        public int H { get; set; }
        public int F => G + H;
        public Node? Parent { get; set; }
        public (byte dir, byte steps)? Action { get; set; }

        public Node(int x, int y, int g, int h, Node? parent = null, (byte dir, byte steps)? action = null)
        {
            X = x;
            Y = y;
            G = g;
            H = h;
            Parent = parent;
            Action = action;
        }

        public int CompareTo(Node? other)
        {
            if (other == null) return 1;
            return F.CompareTo(other.F);
        }
    }

    // 二叉堆实现
    private class BinaryHeap<T> where T : IComparable<T>
    {
        private readonly List<T> _items = new List<T>();

        public int Count => _items.Count;

        public void Insert(T item)
        {
            _items.Add(item);
            int i = _items.Count - 1;
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (_items[i].CompareTo(_items[parent]) >= 0) break;
                Swap(i, parent);
                i = parent;
            }
        }

        public T ExtractMin()
        {
            if (_items.Count == 0) throw new InvalidOperationException("Heap is empty");
            T min = _items[0];
            int lastIndex = _items.Count - 1;
            _items[0] = _items[lastIndex];
            _items.RemoveAt(lastIndex);
            if (_items.Count > 0) Heapify(0);
            return min;
        }

        private void Heapify(int i)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;
            int smallest = i;
            if (left < _items.Count && _items[left].CompareTo(_items[smallest]) < 0)
                smallest = left;
            if (right < _items.Count && _items[right].CompareTo(_items[smallest]) < 0)
                smallest = right;
            if (smallest != i)
            {
                Swap(i, smallest);
                Heapify(smallest);
            }
        }

        private void Swap(int i, int j)
        {
            T temp = _items[i];
            _items[i] = _items[j];
            _items[j] = temp;
        }
    }

    // 计算启发式函数（对角线距离）
    private static int Heuristic(int x1, int y1, int x2, int y2)
    {
        int dx = Math.Abs(x1 - x2);
        int dy = Math.Abs(y1 - y2);
        return 10 * (dx + dy) - 6 * Math.Min(dx, dy);
    }

 


    public static async Task<bool> WaitGoPath(MirGameInstanceModel gameInstance, string map, int x, int y)
    {
        var success = true;
        await Task.Run(() =>
        {
            while (Math.Abs(x - gameInstance.CharacterStatus!.X.GetValueOrDefault()) > 2 && Math.Abs(y - gameInstance.CharacterStatus!.Y.GetValueOrDefault()) > 2)
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


