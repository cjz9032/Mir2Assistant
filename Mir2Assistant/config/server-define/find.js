// var data = [
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0,
//     0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0,
//     0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0,
//     0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0,
//     0, 0, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0,
//     0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
//     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
//     1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
//     1, 1, 1, 1, 1, 1,
// ];
var data = [1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,0,1,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1]


function dfs(data, width, height, startX, startY, targetX, targetY) {
    // 检查起始点是否有效
    if (!isValid(startX, startY, width, height, data)) {
        return {
            path: null,
            error: `起点 (${startX}, ${startY}) 不可通行或超出边界`,
            blockedPositions: [{ x: startX, y: startY }]
        };
    }

    // 检查目标点是否有效
    if (!isValid(targetX, targetY, width, height, data)) {
        return {
            path: null,
            error: `目标点 (${targetX}, ${targetY}) 不可通行或超出边界`,
            blockedPositions: [{ x: targetX, y: targetY }]
        };
    }

    // 如果起点就是终点
    if (startX === targetX && startY === targetY) {
        return {
            path: [{ x: startX, y: startY }],
            error: null,
            blockedPositions: []
        };
    }

    // 初始化访问记录矩阵
    const visited = new Array(width * height).fill(false);
    // 存储路径
    const path = [];
    // 方向数组：上、右、下、左
    const directions = [[0, -1], [1, 0], [0, 1], [-1, 0]];
    // 记录所有尝试过但被阻塞的位置
    const blockedPositions = [];

    // 执行DFS
    const found = dfsRecursive(
        startX, startY, targetX, targetY,
        data, width, height, visited, path, directions, blockedPositions
    );

    if (found) {
        return { path, error: null, blockedPositions: [] };
    } else {
        // 找出所有可达区域的边界位置（这些位置周围有障碍物）
        const boundaryPositions = findBoundaryPositions(visited, width, height, data);
        return {
            path: null,
            error: "无法找到从起点到目标点的路径，可能被障碍物阻断",
            blockedPositions: boundaryPositions
        };
    }
}

function dfsRecursive(x, y, targetX, targetY, data, width, height, visited, path, directions, blockedPositions) {
    // 将当前位置添加到路径
    path.push({ x, y });

    // 标记为已访问
    const index = y * width + x;
    visited[index] = true;

    // 检查是否到达目标
    if (x === targetX && y === targetY) {
        return true;
    }

    // 记录周围的障碍物
    let hasValidNeighbor = false;
    for (const [dx, dy] of directions) {
        const newX = x + dx;
        const newY = y + dy;
        const newIndex = newY * width + newX;

        if (!isValid(newX, newY, width, height, data)) {
            // 记录障碍物位置
            blockedPositions.push({ x: newX, y: newY });
        } else {
            hasValidNeighbor = true;
            if (!visited[newIndex]) {
                if (dfsRecursive(newX, newY, targetX, targetY, data, width, height, visited, path, directions, blockedPositions)) {
                    return true;
                }
            }
        }
    }

    // 如果没有有效邻居，当前位置是死胡同
    if (!hasValidNeighbor) {
        blockedPositions.push({ x, y });
    }

    // 回溯
    path.pop();
    return false;
}

// 检查位置是否有效（在网格范围内且可通行）
function isValid(x, y, width, height, data) {
    if(x === myX && y == myY) return true;
    // 检查是否在网格范围内
    if (x < 0 || x >= width || y < 0 || y >= height) {
        return false;
    }

    // 检查该位置是否可通行（0表示可通行，1表示障碍物）
    const index = y * width + x;
    return data[index] === 0;
}

// 找出可达区域的边界位置
function findBoundaryPositions(visited, width, height, data) {
    const boundary = [];
    const directions = [[0, -1], [1, 0], [0, 1], [-1, 0]];

    for (let y = 0; y < height; y++) {
        for (let x = 0; x < width; x++) {
            const index = y * width + x;
            // 如果是可达区域且有邻居是障碍物或边界
            if (visited[index]) {
                for (const [dx, dy] of directions) {
                    const nx = x + dx;
                    const ny = y + dy;
                    if (!isValid(nx, ny, width, height, data)) {
                        boundary.push({ x, y });
                        break;
                    }
                }
            }
        }
    }
    return boundary;
}

// 输出点阵图
function printGrid(data, width, height, startX, startY, targetX, targetY, path = null, blockedPositions = []) {
    // 创建网格副本用于显示
    const grid = [];
    for (let y = 0; y < height; y++) {
        const row = [];
        for (let x = 0; x < width; x++) {
            const index = y * width + x;
            row.push(data[index] === 1 ? '#' : '.'); // #表示障碍物，.表示可通行
        }
        grid.push(row);
    }

    // 标记阻塞位置
    blockedPositions.forEach(pos => {
        if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height) {
            grid[pos.y][pos.x] = 'X'; // X表示阻塞点
        }
    });

    // 标记路径
    if (path) {
        path.forEach((pos, i) => {
            // 跳过起点和终点，它们会被单独标记
            if (i > 0 && i < path.length - 1) {
                grid[pos.y][pos.x] = '*'; // *表示路径
            }
        });
    }

    // 标记起点和终点
    grid[startY][startX] = 'S'; // S表示起点
    grid[targetY][targetX] = 'T'; // T表示终点

    // 输出网格
    console.log("网格点阵图 (S=起点, T=终点, #=障碍物, *=路径, X=阻塞点):");
    for (let y = 0; y < height; y++) {
        // 只输出可见范围内的行（为了演示，这里限制高度为15行）
        if (y < 99) {
            console.log(grid[y].join(' '));
        } else if (y === 99) {
            console.log("... (省略部分行) ...");
        }
    }
}


var width = 27;
var height = 32;
var myX = 6;
var myY = 10;
var targetX = 2;
var targetY = 12;

// 执行路径查找
const result = dfs(data, width, height, myX, myY, targetX, targetY);

// 输出结果
if (result.path) {
    console.log("找到路径:", result.path);
} else {
    console.log("路径查找失败:", result.error);
    console.log("阻塞位置:", result.blockedPositions);
}

// 输出点阵图
printGrid(data, width, height, myX, myY, targetX, targetY, result.path, result.blockedPositions);
