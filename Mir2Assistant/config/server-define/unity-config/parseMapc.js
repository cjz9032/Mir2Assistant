const fs = require('fs');
const path = require('path');

// 读取mapc文件并解析宽高和障碍物数据
function parseMapcFile(filePath) {
  return new Promise((resolve, reject) => {
    fs.readFile(filePath, (err, data) => {
      if (err) {
        reject(new Error(`文件读取失败: ${err.message}`));
        return;
      }

      try {
        // 解析前两个int32 (小端序)作为宽和高
        const width = data.readInt32LE(0);
        const height = data.readInt32LE(4);
        const totalCells = width * height;
        const obstacleData = [];

        // 从第8字节开始解析障碍物数据
        let offset = 8;
        for (let y = 0; y < height; y++) {
          const row = [];
          for (let x = 0; x < width; x++) {
            // 读取1字节作为障碍物标记 (1=障碍物, 0=可通行)
            const value = data.readUInt8(offset);
            row.push(value === 1);
            offset++;
          }
          obstacleData.push(row);
        }

        resolve({
          width,
          height,
          obstacleData,
          totalCells,
          fileSize: data.length
        });
      } catch (parseError) {
        reject(new Error(`数据解析失败: ${parseError.message}`));
      }
    });
  });
}

// 执行解析并输出结果
const mapcFilePath = path.join(
  __dirname,
  './mapc-out', '0.mapc'
);

parseMapcFile(mapcFilePath)
  .then(result => {
    console.log('地图数据解析成功:');
    console.log(`宽: ${result.width}, 高: ${result.height}`);
    console.log(`总单元格数: ${result.totalCells}, 文件大小: ${result.fileSize}字节`);
    // console.log('障碍物数据示例(前3行):');
    // result.obstacleData.slice(0, 3).forEach((row, index) => {
    //   console.log(`行 ${index}:`, row.slice(0, 10).map(v => v ? '1' : '0').join('') + '...');
    // });
    // 起点
    var myX = 630
    var myY = 611
    var targetX = 100
    var targetY = 200
    // A星寻路算法实现
    // 定义方向数组（八个方向）
    const directions = [
      {dx: 1, dy: 0}, {dx: -1, dy: 0}, {dx: 0, dy: 1}, {dx: 0, dy: -1},
      {dx: 1, dy: 1}, {dx: 1, dy: -1}, {dx: -1, dy: 1}, {dx: -1, dy: -1}
    ];
  
    // 检查坐标是否有效（在地图范围内且非障碍物）
    function isValid(x, y, width, height, obstacleData) {
      return x >= 0 && x < width && y >= 0 && y < height && !obstacleData[y][x];
    }
  
    // 启发函数（曼哈顿距离）
    function heuristic(x1, y1, x2, y2) {
      return Math.abs(x1 - x2) + Math.abs(y1 - y2);
    }
  
    // 检查是否有强制邻居（JPS关键）
    function hasForcedNeighbor(x, y, dx, dy, obstacleData, width, height) {
      // 对角线移动
      if (dx !== 0 && dy !== 0) {
        // 检查垂直方向的强制邻居
        if (isValid(x - dx, y, width, height, obstacleData) && !isValid(x - dx, y + dy, width, height, obstacleData)) {
          return true;
        }
        // 检查水平方向的强制邻居
        if (isValid(x, y - dy, width, height, obstacleData) && !isValid(x + dx, y - dy, width, height, obstacleData)) {
          return true;
        }
      } else {
        // 水平移动
        if (dx !== 0) {
          // 检查上下方向
          if (isValid(x, y + 1, width, height, obstacleData) && !isValid(x + dx, y + 1, width, height, obstacleData)) {
            return true;
          }
          if (isValid(x, y - 1, width, height, obstacleData) && !isValid(x + dx, y - 1, width, height, obstacleData)) {
            return true;
          }
        } else { // 垂直移动
          // 检查左右方向
          if (isValid(x + 1, y, width, height, obstacleData) && !isValid(x + 1, y + dy, width, height, obstacleData)) {
            return true;
          }
          if (isValid(x - 1, y, width, height, obstacleData) && !isValid(x - 1, y + dy, width, height, obstacleData)) {
            return true;
          }
        }
      }
      return false;
    }
  
    // JPS跳跃函数（支持2步优先）
    function jump(x, y, dx, dy, targetX, targetY, obstacleData, width, height) {
      // 尝试2步跳跃
      const nx2 = x + dx * 2;
      const ny2 = y + dy * 2;
      
      // 检查2步是否有效
      if (isValid(nx2, ny2, width, height, obstacleData) && isValid(x + dx, y + dy, width, height, obstacleData)) {
        // 到达目标
        if (nx2 === targetX && ny2 === targetY) {
          return {x: nx2, y: ny2};
        }
        // 有强制邻居则为跳跃点
        if (hasForcedNeighbor(nx2, ny2, dx, dy, obstacleData, width, height)) {
          return {x: nx2, y: ny2};
        }
        // 继续跳跃
        return jump(nx2, ny2, dx, dy, targetX, targetY, obstacleData, width, height);
      } else {
        // 尝试1步跳跃
        const nx = x + dx;
        const ny = y + dy;
        if (isValid(nx, ny, width, height, obstacleData)) {
          if (nx === targetX && ny === targetY) {
            return {x: nx, y: ny};
          }
          if (hasForcedNeighbor(nx, ny, dx, dy, obstacleData, width, height)) {
            return {x: nx, y: ny};
          }
          // 继续跳跃
          return jump(nx, ny, dx, dy, targetX, targetY, obstacleData, width, height);
        }
      }
      return null;
    }
  
    // Node类
    class Node {
      constructor(x, y) {
        this.x = x;
        this.y = y;
        this.g = 0;
        this.h = 0;
        this.f = 0;
        this.parent = null;
      }
    }
  
    // JPS A* 主函数
    function jpsAStar(startX, startY, endX, endY, obstacleData, width, height) {
      const openList = [];
      const closedList = new Set();
  
      const startNode = new Node(startX, startY);
      startNode.g = 0;
      startNode.h = heuristic(startX, startY, endX, endY);
      startNode.f = startNode.g + startNode.h;
      openList.push(startNode);
  
      while (openList.length > 0) {
        // 按f值排序，取最小f值节点
        openList.sort((a, b) => a.f - b.f);
        const currentNode = openList.shift();
        
        // 到达终点
        if (currentNode.x === endX && currentNode.y === endY) {
          // 回溯路径
          const path = [];
          let node = currentNode;
          while (node) {
            path.push({x: node.x, y: node.y});
            node = node.parent;
          }
          return path.reverse(); // 反转路径，从起点到终点
        }
        
        closedList.add(`${currentNode.x},${currentNode.y}`);
        
        // 探索八个方向
        for (const dir of directions) {
          const {dx, dy} = dir;
          // 尝试跳跃
          const jumpPoint = jump(currentNode.x, currentNode.y, dx, dy, endX, endY, obstacleData, width, height);
          if (jumpPoint) {
            const jx = jumpPoint.x;
            const jy = jumpPoint.y;
            const key = `${jx},${jy}`;
            
            if (!closedList.has(key)) {
              const newNode = new Node(jx, jy);
              newNode.g = currentNode.g + 1; // 代价相同，无论是1步还是2步
              newNode.h = heuristic(jx, jy, endX, endY);
              newNode.f = newNode.g + newNode.h;
              newNode.parent = currentNode;
              
              // 检查是否已在开放列表中，如果不在则添加
              const existingNode = openList.find(n => n.x === jx && n.y === jy);
              if (!existingNode) {
                openList.push(newNode);
              } else if (newNode.g < existingNode.g) {
                // 如果找到更优路径，更新节点
                existingNode.g = newNode.g;
                existingNode.f = newNode.f;
                existingNode.parent = newNode.parent;
              }
            }
          }
        }
      }
      
      // 未找到路径
      return null;
    }
  
    // 执行寻路
    const path = jpsAStar(myX, myY, targetX, targetY, result.obstacleData, result.width, result.height);
  
    if (path) {
      console.log('寻路成功，路径点:');
      path.forEach((point, index) => {
        console.log(`步骤 ${index + 1}: (${point.x}, ${point.y})`);
      });
    } else {
      console.log('无法找到路径');
    }
  })
  .catch(error => {
    console.error('解析失败:', error.message);
  });