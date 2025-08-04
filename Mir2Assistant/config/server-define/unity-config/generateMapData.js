const fs = require('fs');
const path = require('path');

// 读取 JSON 文件
const jsonContent = fs.readFileSync('map-entry.json', 'utf8');
const mapEntry = JSON.parse(jsonContent);

// 准备数据
const mapNames = {};
const mapIdToIndex = {};
const indexToMapId = [];
const connections = [];

// 先收集所有唯一的地图ID
const allMapIds = new Set();
mapEntry.list.forEach(map => {
    mapNames[map.id.toUpperCase()] = map.name;
    allMapIds.add(map.id.toUpperCase());
    
    map.pos.forEach(pos => {
        allMapIds.add(pos.from.mid);
        allMapIds.add(pos.to.mid);
    });
});

// 创建地图ID索引映射
Array.from(allMapIds).sort().forEach((mapId, index) => {
    mapIdToIndex[mapId] = index;
    indexToMapId[index] = mapId;
});

// 收集连接数据
mapEntry.list.forEach(map => {
    map.pos.forEach(pos => {
        connections.push({
            fromIndex: mapIdToIndex[pos.from.mid],
            fromX: pos.from.x,
            fromY: pos.from.y,
            toIndex: mapIdToIndex[pos.to.mid],
            toX: pos.to.x,
            toY: pos.to.y
        });
    });
});

// 构建邻接表数组
const maxIndex = indexToMapId.length;
const adjacencyList = new Array(maxIndex);
for (let i = 0; i < maxIndex; i++) {
    adjacencyList[i] = [];
}

connections.forEach(conn => {
    adjacencyList[conn.fromIndex].push([conn.toIndex, conn.fromX, conn.fromY, conn.toX, conn.toY]);
});

console.log(`收集完成！共 ${indexToMapId.length} 个地图，${connections.length} 个连接`);

// 生成 C# 代码 - 使用数组而不是字典，提高访问速度
const code = `using System.Collections.Generic;
using Mir2Assistant.Models.MapPathFinding;

namespace Mir2Assistant.Generated
{
    public static class MapData
    {
        public static Dictionary<string, string> MapNames { get; } = new()
        {
${Object.entries(mapNames).map(([id, name]) => `            ["${id}"] = "${name}",`).join('\n')}
        };

        // 地图ID到索引的映射
        private static readonly Dictionary<string, int> _mapIdToIndex = new()
        {
${Object.entries(mapIdToIndex).map(([mapId, index]) => `            ["${mapId}"] = ${index},`).join('\n')}
        };

        // 索引到地图ID的映射 - 使用数组提高访问速度
        private static readonly string[] _indexToMapId = new string[]
        {
${indexToMapId.map(mapId => `            "${mapId}",`).join('\n')}
        };

        // 邻接表 - 使用数组提高访问速度: [toIndex, fromX, fromY, toX, toY]
        private static readonly int[][][] _adjacencyList = new int[][][]
        {
${adjacencyList.map((conns, index) => `            new int[][] // Index ${index}: ${indexToMapId[index]}
            {
${conns.map(conn => `                new int[] { ${conn.join(', ')} },`).join('\n')}
            },`).join('\n')}
        };

        public static int GetMapIndex(string mapId) => _mapIdToIndex.TryGetValue(mapId, out var index) ? index : -1;
        
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static string GetMapId(int index) => index >= 0 && index < _indexToMapId.Length ? _indexToMapId[index] : "";
        
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int[][] GetConnections(int mapIndex) => mapIndex >= 0 && mapIndex < _adjacencyList.Length ? _adjacencyList[mapIndex] : new int[0][];
        
        public static int MapCount => _indexToMapId.Length;
    }
}`;

// 创建输出目录
const outputDir = path.join('..', '..', '..', 'Generated');
if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true });
}

// 写入文件
fs.writeFileSync(path.join(outputDir, 'MapData.cs'), code);
console.log(`生成完成！超高性能图结构已生成 - 使用数组访问，内联方法优化！`); 