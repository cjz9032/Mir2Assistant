const fs = require('fs');
const path = require('path');
const mapEntry = require('./map-entry').default;
const npcMapEntry = require('./map-by-npc-entry').default;

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

// 处理NPC传送数据
npcMapEntry.list.forEach(map => {
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
            toY: pos.to.y,
            npcName: "",
            npcCmds: ""
        });
    });
});

// 收集NPC传送连接数据
npcMapEntry.list.forEach(map => {
    map.pos.forEach(pos => {
        // 支持 from 和 to 都有 npcName/npcCmds
        const fromNpcName = pos.npcName || pos.from.npcName || "";
        const fromNpcCmds = pos.npcCmds || pos.from.npcCmds || "";
        const toNpcName = pos.to.npcName || "";
        const toNpcCmds = pos.to.npcCmds || "";
        
        connections.push({
            fromIndex: mapIdToIndex[pos.from.mid],
            fromX: pos.from.x,
            fromY: pos.from.y,
            fromNpcName: fromNpcName,
            fromNpcCmds: fromNpcCmds,
            toIndex: mapIdToIndex[pos.to.mid],
            toX: pos.to.x,
            toY: pos.to.y,
            toNpcName: toNpcName,
            toNpcCmds: toNpcCmds
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
    adjacencyList[conn.fromIndex].push([
        conn.toIndex, 
        conn.fromX, 
        conn.fromY, 
        conn.fromNpcName || "", 
        conn.fromNpcCmds || "", 
        conn.toX, 
        conn.toY, 
        conn.toNpcName || "", 
        conn.toNpcCmds || ""
    ]);
});

console.log(`收集完成！共 ${indexToMapId.length} 个地图，${connections.length} 个连接`);

// 生成 C# 代码 - 使用数组而不是字典，提高访问速度
const code = `using System.Collections.Generic;
using Mir2Assistant.Common.Models.MapPathFinding;

namespace Mir2Assistant.Common.Generated
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

        // 邻接表 - 使用数组提高访问速度: [toIndex, fromX, fromY, fromNpcName, fromNpcCmds, toX, toY, toNpcName, toNpcCmds]
        private static readonly object[][][] _adjacencyList = new object[][][]
        {
${adjacencyList.map((conns, index) => `            new object[][] // Index ${index}: ${indexToMapId[index]}
            {
${conns.map(conn => `                new object[] { ${conn[0]}, ${conn[1]}, ${conn[2]}, "${conn[3] || ""}", "${conn[4] || ""}", ${conn[5]}, ${conn[6]}, "${conn[7] || ""}", "${conn[8] || ""}" },`).join('\n')}
            },`).join('\n')}
        };

        public static int GetMapIndex(string mapId) => _mapIdToIndex.TryGetValue(mapId, out var index) ? index : -1;
        
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static string GetMapId(int index) => index >= 0 && index < _indexToMapId.Length ? _indexToMapId[index] : "";
        
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static object[][] GetConnections(int mapIndex) => mapIndex >= 0 && mapIndex < _adjacencyList.Length ? _adjacencyList[mapIndex] : new object[0][];
        
        // 获取连接信息，包含NPC传送数据
        public static MapPosition[] GetMapConnections(string mapId)
        {
            var index = GetMapIndex(mapId);
            if (index == -1) return new MapPosition[0];
            
            var connections = GetConnections(index);
            var result = new MapPosition[connections.Length];
            
            for (int i = 0; i < connections.Length; i++)
            {
                var conn = connections[i];
                result[i] = new MapPosition
                {
                    MapId = GetMapId((int)conn[0]),
                    X = (int)conn[3],
                    Y = (int)conn[4],
                    NpcName = (string)conn[5],
                    NpcCmds = (string)conn[6]
                };
            }
            
            return result;
        }
        
        public static int MapCount => _indexToMapId.Length;

        /// <summary>
        /// 获取指定地图的所有传送点坐标（from位置）
        /// 用于在寻路时根据场景避开或接近这些传送点
        /// </summary>
        /// <param name="mapId">地图ID</param>
        /// <returns>传送点坐标列表 (x, y)</returns>
        public static List<(int x, int y)> GetPortalPoints(string mapId)
        {
            var index = GetMapIndex(mapId);
            if (index == -1) return new List<(int x, int y)>();
            
            var connections = GetConnections(index);
            var portalPoints = new List<(int x, int y)>(connections.Length);
            
            for (int i = 0; i < connections.Length; i++)
            {
                var conn = connections[i];
                // conn格式: [toIndex, fromX, fromY, fromNpcName, fromNpcCmds, toX, toY, toNpcName, toNpcCmds]
                int fromX = (int)conn[1];
                int fromY = (int)conn[2];
                portalPoints.Add((fromX, fromY));
            }
            
            return portalPoints;
        }
    }
}`;

// 创建输出目录
const outputDir = path.join('..', '..', '..', '..', 'Mir2Assistant.Common', 'Generated');
if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true });
}

// 写入文件
fs.writeFileSync(path.join(outputDir, 'MapData.cs'), code);
console.log(`生成完成！超高性能图结构已生成 - 使用数组访问，内联方法优化！`);
console.log(`输出路径: ${path.resolve(outputDir)}`);
console.log(`包含 NPC 传送支持，共 ${connections.filter(c => c.npcName).length} 个 NPC 传送点`); 