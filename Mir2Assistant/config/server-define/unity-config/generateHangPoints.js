const fs = require('fs');
const path = require('path');

// 读取所有地图数据
function loadAllMapData() {
    const mapDir = path.join(__dirname, 'map');
    const mapFiles = fs.readdirSync(mapDir).filter(file => file.endsWith('.json'));
    const allMapData = {};
    
    for (const file of mapFiles) {
        const mapId = path.basename(file, '.json').toUpperCase();
        const mapPath = path.join(mapDir, file);
        try {
            const mapData = JSON.parse(fs.readFileSync(mapPath, 'utf8'));
            if (mapData.width && mapData.height && mapData.c) {
                allMapData[mapId] = mapData;
            }
        } catch (error) {
            console.log(`跳过无效地图文件: ${file} - ${error.message}`);
        }
    }
    
    return allMapData;
}

// 将一维数组转换为二维数组便于处理
function get2DMap(data, width, height) {
    const map = [];
    for (let y = 0; y < height; y++) {
        map[y] = [];
        for (let x = 0; x < width; x++) {
            map[y][x] = data[y * width + x];
        }
    }
    return map;
}

// 检查位置是否可通过
function isWalkable(map, x, y) {
    if (x < 0 || x >= map[0].length || y < 0 || y >= map.length) {
        return false;
    }
    return map[y][x] === 0;
}

// 计算两点之间的距离
function distance(p1, p2) {
    return Math.sqrt((p1.x - p2.x) ** 2 + (p1.y - p2.y) ** 2);
}

// 寻找可通过的区域
function findWalkableAreas(map) {
    const walkablePoints = [];
    for (let y = 0; y < map.length; y++) {
        for (let x = 0; x < map[y].length; x++) {
            if (isWalkable(map, x, y)) {
                walkablePoints.push({ x, y });
            }
        }
    }
    return walkablePoints;
}

// 生成挂机点，使用网格采样确保覆盖整个地图
function generateHangPoints(walkablePoints, targetDistance = 10, mapId = "") {
    if (walkablePoints.length === 0) return [];
    
    // 找到地图边界
    let minX = Infinity, maxX = -Infinity;
    let minY = Infinity, maxY = -Infinity;
    
    for (const point of walkablePoints) {
        minX = Math.min(minX, point.x);
        maxX = Math.max(maxX, point.x);
        minY = Math.min(minY, point.y);
        maxY = Math.max(maxY, point.y);
    }
    
    const mapWidth = maxX - minX + 1;
    const mapHeight = maxY - minY + 1;
    
    console.log(`  地图边界: (${minX}, ${minY}) 到 (${maxX}, ${maxY}), 尺寸: ${mapWidth} x ${mapHeight}`);
    
    // 创建可通过点的快速查找集合
    const walkableSet = new Set();
    for (const point of walkablePoints) {
        walkableSet.add(`${point.x},${point.y}`);
    }
    
    const hangPoints = [];
    
    // 使用网格采样，确保覆盖整个地图
    const gridSpacing = targetDistance;
    
    for (let y = minY; y <= maxY; y += gridSpacing) {
        for (let x = minX; x <= maxX; x += gridSpacing) {
            // 在当前网格点周围寻找最近的可通过点
            let bestPoint = null;
            let minDistance = Infinity;
            
            // 搜索范围
            const searchRadius = gridSpacing;
            
            for (let dy = -searchRadius; dy <= searchRadius; dy++) {
                for (let dx = -searchRadius; dx <= searchRadius; dx++) {
                    const testX = Math.round(x + dx);
                    const testY = Math.round(y + dy);
                    
                    if (walkableSet.has(`${testX},${testY}`)) {
                        const dist = Math.sqrt(dx * dx + dy * dy);
                        if (dist < minDistance) {
                            minDistance = dist;
                            bestPoint = { x: testX, y: testY };
                        }
                    }
                }
            }
            
            if (bestPoint) {
                // 检查是否与已有点太近
                let tooClose = false;
                for (const existing of hangPoints) {
                    const dist = distance(bestPoint, existing);
                    if (dist < targetDistance * 0.7) {
                        tooClose = true;
                        break;
                    }
                }
                
                if (!tooClose) {
                    hangPoints.push(bestPoint);
                }
            }
        }
    }
    
    console.log(`  网格采样生成 ${hangPoints.length} 个挂机点`);
    
    // 如果点数太少，降低距离要求再次采样
    if (hangPoints.length < 20 && targetDistance > 5) {
        console.log(`  挂机点数量不足，降低间距要求重新采样`);
        return generateHangPoints(walkablePoints, targetDistance * 0.8, mapId);
    }
    
    return hangPoints;
}

// 优化挂机点路径，确保连线到底不绕路
function optimizeHangPointsPath(hangPoints) {
    if (hangPoints.length < 3) return hangPoints;
    
    // 使用最近邻算法构建连贯路径
    const optimized = [hangPoints[0]];
    const remaining = [...hangPoints.slice(1)];
    
    while (remaining.length > 0) {
        const current = optimized[optimized.length - 1];
        let nearestIndex = 0;
        let nearestDistance = distance(current, remaining[0]);
        
        // 找到最近的下一个点
        for (let i = 1; i < remaining.length; i++) {
            const dist = distance(current, remaining[i]);
            if (dist < nearestDistance) {
                nearestDistance = dist;
                nearestIndex = i;
            }
        }
        
        optimized.push(remaining[nearestIndex]);
        remaining.splice(nearestIndex, 1);
    }
    
    // 检查路径是否有明显的交叉或绕路，如果有则尝试2-opt优化
    return optimizeWith2Opt(optimized);
}

// 2-opt算法优化路径，减少交叉和绕路
function optimizeWith2Opt(points) {
    if (points.length < 4) return points;
    
    let improved = true;
    let route = [...points];
    let iterations = 0;
    const maxIterations = Math.min(100, points.length * 2); // 限制迭代次数
    
    while (improved && iterations < maxIterations) {
        improved = false;
        iterations++;
        
        for (let i = 1; i < route.length - 2; i++) {
            for (let j = i + 1; j < route.length; j++) {
                // 计算当前路径长度
                const currentDist = distance(route[i - 1], route[i]) + distance(route[j], route[(j + 1) % route.length]);
                
                // 计算交换后的路径长度
                const newDist = distance(route[i - 1], route[j]) + distance(route[i], route[(j + 1) % route.length]);
                
                // 如果交换后路径更短，则进行交换
                if (newDist < currentDist) {
                    // 反转i到j之间的路径
                    const newRoute = [...route];
                    for (let k = 0; k <= j - i; k++) {
                        newRoute[i + k] = route[j - k];
                    }
                    route = newRoute;
                    improved = true;
                }
            }
        }
    }
    
    if (iterations > 1) {
        console.log(`  路径优化：执行了 ${iterations} 次2-opt优化`);
    }
    
    return route;
}

// 生成C#代码
function generateCSharpCode(allMapHangPoints) {
    const mapEntries = [];
    let totalHangPoints = 0;
    
    for (const [mapId, hangPoints] of Object.entries(allMapHangPoints)) {
        if (hangPoints.length > 0) {
            const pointsCode = hangPoints.map(p => `                new int[] { ${p.x}, ${p.y} }`).join(',\n');
            mapEntries.push(`            ["${mapId}"] = new int[][]
            {
${pointsCode}
            }`);
            totalHangPoints += hangPoints.length;
        }
    }
    
    const dictionaryCode = mapEntries.join(',\n');
    
    return `using System.Collections.Generic;
using System.Linq;

namespace Mir2Assistant.Common.Generated
{
    /// <summary>
    /// 挂机点数据类，包含所有地图的挂机点信息
    /// 总共 ${Object.keys(allMapHangPoints).length} 个地图，${totalHangPoints} 个挂机点
    /// </summary>
    public static class HangPointData
    {
        /// <summary>
        /// 挂机点数据字典，以地图ID为key，挂机点数组为value
        /// 格式: Dictionary<string, int[][]>，其中 int[] { x, y }
        /// </summary>
        public static readonly Dictionary<string, int[][]> HangPoints = new Dictionary<string, int[][]>
        {
${dictionaryCode}
        };
        
        /// <summary>
        /// 获取指定地图的挂机点
        /// </summary>
        /// <param name="mapId">地图ID</param>
        /// <returns>挂机点数组，格式为 int[] { x, y }，如果地图不存在则返回空数组</returns>
        public static int[][] GetHangPoints(string mapId)
        {
            return HangPoints.TryGetValue(mapId, out var points) ? points : new int[0][];
        }
        
        /// <summary>
        /// 获取下一个挂机点的索引（循环）
        /// </summary>
        /// <param name="currentIndex">当前挂机点索引</param>
        /// <param name="mapId">地图ID</param>
        /// <returns>下一个挂机点索引</returns>
        public static int GetNextHangPointIndex(int currentIndex, string mapId)
        {
            var points = GetHangPoints(mapId);
            if (points.Length == 0) return 0;
            return (currentIndex + 1) % points.Length;
        }
        
        /// <summary>
        /// 获取挂机点总数
        /// </summary>
        /// <param name="mapId">地图ID</param>
        /// <returns>挂机点总数</returns>
        public static int GetHangPointCount(string mapId)
        {
            return GetHangPoints(mapId).Length;
        }
        
        /// <summary>
        /// 获取指定索引的挂机点坐标
        /// </summary>
        /// <param name="index">挂机点索引</param>
        /// <param name="mapId">地图ID</param>
        /// <returns>坐标数组 int[] { x, y }，如果索引无效则返回 null</returns>
        public static int[]? GetHangPoint(int index, string mapId)
        {
            var points = GetHangPoints(mapId);
            if (index < 0 || index >= points.Length) return null;
            return points[index];
        }
        
        /// <summary>
        /// 检查指定地图是否有挂机点数据
        /// </summary>
        /// <param name="mapId">地图ID</param>
        /// <returns>如果存在挂机点数据则返回true</returns>
        public static bool HasHangPoints(string mapId)
        {
            return HangPoints.ContainsKey(mapId);
        }
        
        /// <summary>
        /// 获取所有有挂机点数据的地图ID
        /// </summary>
        /// <returns>地图ID数组</returns>
        public static string[] GetAllMapIds()
        {
            return HangPoints.Keys.ToArray();
        }
    }
}`;
}

// 主函数
function main() {
    console.log('=== 挂机点生成器 ===');
    
    // 加载所有地图数据
    const allMapData = loadAllMapData();
    const mapIds = Object.keys(allMapData);
    console.log(`找到 ${mapIds.length} 个有效地图文件`);
    
    const allMapHangPoints = {};
    let totalProcessed = 0;
    let totalHangPoints = 0;
    
    // 处理每个地图
    for (const [mapId, mapData] of Object.entries(allMapData)) {
        const { width, height, c: data } = mapData;
        
        console.log(`\n处理地图 ${mapId} (${width} x ${height})`);
        
        // 转换为2D地图
        const map2D = get2DMap(data, width, height);
        
        // 寻找可通过的区域
        const walkablePoints = findWalkableAreas(map2D);
        console.log(`  可通过的点数: ${walkablePoints.length}`);
        
        if (walkablePoints.length === 0) {
            console.log(`  跳过地图 ${mapId}: 没有可通过的区域`);
            allMapHangPoints[mapId] = [];
            continue;
        }
        
        // 根据地图大小调整目标距离
        const mapSize = Math.sqrt(walkablePoints.length);
        let targetDistance = 15;
        var minSize = targetDistance*1.5;
        var midSize = targetDistance*3;
        var maxSize = targetDistance*8;
        
        if (mapSize < minSize) {
            // 非常小的地图
            targetDistance = Math.max(5, mapSize * 0.4);
        } else if (mapSize < midSize) {
            // 小地图
            targetDistance = Math.max(7, mapSize * 0.3);
        } else if (mapSize > maxSize) {
            // 大地图，增加间距
            targetDistance = targetDistance * 1.5;
        }
        
        console.log(`  地图规模: ${mapSize.toFixed(1)}, 目标间距: ${targetDistance.toFixed(1)} 单位`)
        
        // 生成挂机点
        const hangPoints = generateHangPoints(walkablePoints, targetDistance, mapId);
        console.log(`  生成的挂机点数: ${hangPoints.length}`);
        
        if (hangPoints.length === 0) {
            console.log(`  跳过地图 ${mapId}: 无法生成挂机点`);
            allMapHangPoints[mapId] = [];
            continue;
        }
        
        // 优化路径
        const optimizedHangPoints = optimizeHangPointsPath(hangPoints);
        console.log(`  优化后的挂机点数: ${optimizedHangPoints.length}`);
        
        // 计算平均距离
        if (optimizedHangPoints.length > 1) {
            let totalDistance = 0;
            for (let i = 0; i < optimizedHangPoints.length; i++) {
                const current = optimizedHangPoints[i];
                const next = optimizedHangPoints[(i + 1) % optimizedHangPoints.length];
                totalDistance += distance(current, next);
            }
            const avgDistance = totalDistance / optimizedHangPoints.length;
            console.log(`  挂机点间平均距离: ${avgDistance.toFixed(2)} 单位`);
        }
        
        allMapHangPoints[mapId] = optimizedHangPoints;
        totalProcessed++;
        totalHangPoints += optimizedHangPoints.length;
    }
    
    console.log(`\n=== 处理完成 ===`);
    console.log(`成功处理 ${totalProcessed} 个地图`);
    console.log(`总共生成 ${totalHangPoints} 个挂机点`);
    
    // 生成C#代码
    const csharpCode = generateCSharpCode(allMapHangPoints);
    
    // 写入文件
    const outputPath = path.join(__dirname, '..', '..', '..', '..', 'Mir2Assistant.Common', 'Generated', 'HangPointData.cs');
    
    // 确保目录存在
    const outputDir = path.dirname(outputPath);
    if (!fs.existsSync(outputDir)) {
        fs.mkdirSync(outputDir, { recursive: true });
    }
    
    fs.writeFileSync(outputPath, csharpCode, 'utf8');
    console.log(`挂机点数据已生成到: ${outputPath}`);
    
    // 输出统计信息
    console.log('\n=== 地图统计 ===');
    const sortedMaps = Object.entries(allMapHangPoints)
        .filter(([_, points]) => points.length > 0)
        .sort(([a], [b]) => a.localeCompare(b))
        .slice(0, 10);
    
    sortedMaps.forEach(([mapId, points]) => {
        console.log(`  ${mapId}: ${points.length} 个挂机点`);
    });
    
    if (Object.keys(allMapHangPoints).length > 10) {
        console.log(`  ... 还有 ${Object.keys(allMapHangPoints).length - 10} 个地图`);
    }
}

// 运行主函数
main(); 