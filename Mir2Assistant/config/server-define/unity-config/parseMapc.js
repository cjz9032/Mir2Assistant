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

// 方向定义: 上、右上、右、右下、下、左下、左、左上
const DIRECTIONS = [
  { x: 0, y: -1 },  // 上
  { x: 1, y: -1 },  // 右上
  { x: 1, y: 0 },   // 右
  { x: 1, y: 1 },   // 右下
  { x: 0, y: 1 },   // 下
  { x: -1, y: 1 },  // 左下
  { x: -1, y: 0 },  // 左
  { x: -1, y: -1 }  // 左上
];

class Node {
  constructor(x, y, parent = null) {
    this.x = x;
    this.y = y;
    this.parent = parent;
    this.g = 0;
    this.h = 0;
    this.f = 0;
    this.stepSize = 2; // 默认2步移动
  }
}

function manhattanDistance(x1, y1, x2, y2) {
  return Math.abs(x1 - x2) + Math.abs(y1 - y2);
}

function isValidPosition(x, y, width, height) {
  return x >= 0 && x < width && y >= 0 && y < height;
}

function canMove(x, y, dx, dy, stepSize, obstacleData, width, height, force2Step = false) {
  if (force2Step && stepSize === 1) {
    return false;
  }

  if (stepSize === 1) {
    const newX = x + dx;
    const newY = y + dy;
    return isValidPosition(newX, newY, width, height) && !obstacleData[newY][newX];
  } else {
    // 检查2步路径上的所有格子
    const midX = x + dx;
    const midY = y + dy;
    const endX = x + dx * 2;
    const endY = y + dy * 2;

    return isValidPosition(midX, midY, width, height) &&
           isValidPosition(endX, endY, width, height) &&
           !obstacleData[midY][midX] &&
           !obstacleData[endY][endX];
  }
}

function findJumpPoint(x, y, dx, dy, obstacleData, width, height, targetX, targetY, force2Step = false) {
  // 首先尝试2步移动
  let stepSize = 2;
  let newX = x + dx * stepSize;
  let newY = y + dy * stepSize;

  // 如果2步不行，且不是强制2步模式，则尝试1步
  if (!canMove(x, y, dx, dy, 2, obstacleData, width, height, force2Step)) {
    if (force2Step) {
      return null;
    }
    stepSize = 1;
    newX = x + dx;
    newY = y + dy;
    
    if (!canMove(x, y, dx, dy, 1, obstacleData, width, height, force2Step)) {
      return null;
    }
  }

  // 到达目标点
  if (newX === targetX && newY === targetY) {
    return { x: newX, y: newY, stepSize };
  }

  // 检查是否是跳点
  for (let i = 0; i < DIRECTIONS.length; i++) {
    const checkDx = DIRECTIONS[i].x;
    const checkDy = DIRECTIONS[i].y;
    
    // 跳过来源方向
    if (checkDx === -dx && checkDy === -dy) continue;
    
    // 检查相邻的障碍物
    if (isValidPosition(newX + checkDx, newY + checkDy, width, height) &&
        !canMove(newX, newY, checkDx, checkDy, 1, obstacleData, width, height, force2Step) &&
        canMove(newX + checkDx, newY + checkDy, dx, dy, 1, obstacleData, width, height, force2Step)) {
      return { x: newX, y: newY, stepSize };
    }
  }

  // 继续在同一方向搜索
  return findJumpPoint(newX, newY, dx, dy, obstacleData, width, height, targetX, targetY, force2Step);
}

function findPath(startX, startY, targetX, targetY, obstacleData, force2Step = false) {
  const width = obstacleData[0].length;
  const height = obstacleData.length;
  
  const openSet = new Set();
  const closedSet = new Set();
  const startNode = new Node(startX, startY);
  startNode.h = manhattanDistance(startX, startY, targetX, targetY);
  startNode.f = startNode.h;
  
  openSet.add(JSON.stringify({ x: startX, y: startY }));
  const nodeMap = new Map();
  nodeMap.set(JSON.stringify({ x: startX, y: startY }), startNode);
  
  // 添加统计信息
  let stats = {
    nodesExplored: 0,
    jumpPointsChecked: 0,
    maxOpenSetSize: 0,
    keyPositionsFound: 0 // This will be updated in the loop
  };
  
  while (openSet.size > 0) {
    stats.maxOpenSetSize = Math.max(stats.maxOpenSetSize, openSet.size);
    
    let currentKey = Array.from(openSet).reduce((a, b) => {
      return nodeMap.get(a).f < nodeMap.get(b).f ? a : b;
    });
    
    const current = nodeMap.get(currentKey);
    stats.nodesExplored++;
    
    if (current.x === targetX && current.y === targetY) {
      // 构建详细路径，包括跳点之间的中间步骤
      const detailedPath = [];
      let node = current;
      const rawPath = [];
      
      while (node) {
        rawPath.unshift({
          x: node.x,
          y: node.y,
          stepSize: node.stepSize,
          fromStepSize: node.fromStepSize || 2
        });
        node = node.parent;
      }
      
      // 为每对跳点生成详细路径
      for (let i = 0; i < rawPath.length - 1; i++) {
        const current = rawPath[i];
        const next = rawPath[i + 1];
        
        // 添加当前点，使用到达这个点的步数
        detailedPath.push({
          x: current.x,
          y: current.y,
          type: `跳点(${next.fromStepSize}步)`,
          stepSize: next.fromStepSize
        });
        
        // 计算方向
        const dx = Math.sign(next.x - current.x);
        const dy = Math.sign(next.y - current.y);
        
        // 计算距离
        const distance = Math.max(
          Math.abs(next.x - current.x),
          Math.abs(next.y - current.y)
        );
        
        // 根据stepSize生成中间点
        let currentX = current.x;
        let currentY = current.y;
        
        while (currentX !== next.x || currentY !== next.y) {
          const stepSize = canMove(currentX, currentY, dx, dy, 2, obstacleData, width, height) ? 2 : 1;
          
          currentX += dx * stepSize;
          currentY += dy * stepSize;
          
          // 确保不会超过目标点
          if (Math.abs(currentX - current.x) > Math.abs(next.x - current.x)) {
            currentX = next.x;
          }
          if (Math.abs(currentY - current.y) > Math.abs(next.y - current.y)) {
            currentY = next.y;
          }
          
          if (currentX !== next.x || currentY !== next.y) {
            detailedPath.push({
              x: currentX,
              y: currentY,
              type: `中间点(${stepSize}步)`,
              stepSize: stepSize
            });
          }
        }
      }
      
      // 添加终点
      const lastPoint = rawPath[rawPath.length - 1];
      detailedPath.push({
        x: lastPoint.x,
        y: lastPoint.y,
        type: '终点',
        stepSize: lastPoint.stepSize
      });
      
      return { path: detailedPath, stats };
    }
    
    openSet.delete(currentKey);
    closedSet.add(currentKey);
    
    // 检查所有8个方向
    for (const dir of DIRECTIONS) {
      stats.jumpPointsChecked++;
      const jumpPoint = findJumpPoint(
        current.x, current.y,
        dir.x, dir.y,
        obstacleData,
        width, height,
        targetX, targetY,
        force2Step,
        null // Removed keyPositions as it's now integrated into canMove
      );
      
      if (!jumpPoint) continue;
      
      const newKey = JSON.stringify({ x: jumpPoint.x, y: jumpPoint.y });
      if (closedSet.has(newKey)) continue;
      
      const gScore = current.g + manhattanDistance(current.x, current.y, jumpPoint.x, jumpPoint.y);
      let neighbor = nodeMap.get(newKey);
      
      if (!neighbor) {
        neighbor = new Node(jumpPoint.x, jumpPoint.y, current);
        neighbor.stepSize = jumpPoint.stepSize;
        nodeMap.set(newKey, neighbor);
      }
      
      if (!openSet.has(newKey)) {
        openSet.add(newKey);
      } else if (gScore >= neighbor.g) {
        continue;
      }
      
      neighbor.parent = current;
      neighbor.g = gScore;
      neighbor.h = manhattanDistance(jumpPoint.x, jumpPoint.y, targetX, targetY);
      neighbor.f = neighbor.g + neighbor.h;
    }
  }
  
  return { path: null, stats };
}

// 区域大小配置
const REGION_SIZE = 50; // 每个区域的边长

// 导航系统类
class NavigationSystem {
  constructor(mapData) {
    this.width = mapData[0].length;
    this.height = mapData.length;
    this.mapData = mapData;
    this.regions = new Map(); // 存储区域信息
    this.navPoints = new Set(); // 存储导航点
    this.connections = new Map(); // 存储区域间连接
    
    // 添加缓存
    this.navPointsArray = []; // 导航点数组形式缓存
    this.navPointConnections = new Map(); // 导航点直接连接缓存
    this.regionNavPoints = new Map(); // 区域到导航点的映射缓存
    
    this.REGION_SIZE = 120; // 减小区域大小以增加连通性
    this.NAV_POINT_INTERVAL = 8; // 减小间隔以增加导航点
    this.MAX_CONNECTIONS = 12; // 增加连接数以提高连通性
    this.MAX_CONNECTION_DIST = this.REGION_SIZE * 1.5; // 增加最大连接距离
    
    // 添加性能统计
    this.stats = {
      preprocess: {
        regionDivisionTime: 0,
        navPointDetectionTime: 0,
        connectionBuildTime: 0,
        totalNodes: 0,
        totalRegions: 0,
        totalNavPoints: 0,
        totalConnections: 0
      },
      pathfinding: {
        localPathNodesExplored: 0,
        navPathNodesExplored: 0,
        maxOpenSetSize: 0,
        segmentCount: 0,
        navPointSearchTime: 0,
        navPathTime: 0,
        localPathTime: 0
      }
    };
    
    this.preprocessMap();
  }

  // 判断一个点是否是关键通道点
  isKeyPassage(x, y) {
    if (!this.isWalkable(x, y)) return false;

    // 检查是否在区域边界或边界附近
    const onBorderX = x % this.REGION_SIZE === 0 || 
                     x % this.REGION_SIZE === this.REGION_SIZE - 1;
    const onBorderY = y % this.REGION_SIZE === 0 || 
                     y % this.REGION_SIZE === this.REGION_SIZE - 1;
    
    // 在边界上按间隔放置导航点
    if ((onBorderX && y % this.NAV_POINT_INTERVAL === 0) ||
        (onBorderY && x % this.NAV_POINT_INTERVAL === 0)) {
      return true;
    }

    // 检查是否是关键位置（周围有障碍但可通行）
    if (x % (this.NAV_POINT_INTERVAL * 2) === 0 && 
        y % (this.NAV_POINT_INTERVAL * 2) === 0) {
      let walkable = 0;
      let blocked = 0;
      for (const dir of DIRECTIONS) {
        const nx = x + dir.x;
        const ny = y + dir.y;
        if (this.isWalkable(nx, ny)) {
          walkable++;
        } else {
          blocked++;
        }
      }
      // 如果是通道或拐角
      if (walkable >= 3 && blocked >= 2) {
        return true;
      }
    }

    return false;
  }

  // 获取点所在的区域ID
  getRegionId(x, y) {
    const regionX = Math.floor(x / this.REGION_SIZE);
    const regionY = Math.floor(y / this.REGION_SIZE);
    return `${regionX},${regionY}`;
  }

  // 检查位置是否可通行
  isWalkable(x, y) {
    return x >= 0 && x < this.width && 
           y >= 0 && y < this.height && 
           !this.mapData[y][x];
  }

  // 预处理地图
  preprocessMap() {
    // 1. 划分区域
    const regionStart = process.hrtime();
    for (let y = 0; y < this.height; y++) {
      for (let x = 0; x < this.width; x++) {
        if (this.isWalkable(x, y)) {
          const regionId = this.getRegionId(x, y);
          if (!this.regions.has(regionId)) {
            this.regions.set(regionId, new Set());
          }
          this.regions.get(regionId).add(`${x},${y}`);
          this.stats.preprocess.totalNodes++;
        }
      }
    }
    const regionEnd = process.hrtime(regionStart);
    this.stats.preprocess.regionDivisionTime = regionEnd[0] * 1000 + regionEnd[1] / 1000000;
    this.stats.preprocess.totalRegions = this.regions.size;

    // 2. 找出关键通道点
    const navStart = process.hrtime();
    for (const [regionId, points] of this.regions) {
      const regionNavPoints = new Set();
      
      for (const pointStr of points) {
        const [x, y] = pointStr.split(',').map(Number);
        if (this.isKeyPassage(x, y)) {
          this.navPoints.add(pointStr);
          regionNavPoints.add(pointStr);
          this.stats.preprocess.totalNavPoints++;
        }
      }
      
      if (regionNavPoints.size > 0) {
        this.regionNavPoints.set(regionId, Array.from(regionNavPoints));
      }
    }
    const navEnd = process.hrtime(navStart);
    this.stats.preprocess.navPointDetectionTime = navEnd[0] * 1000 + navEnd[1] / 1000000;

    // 3. 建立导航点连接
    const connStart = process.hrtime();
    this.navPointsArray = Array.from(this.navPoints);
    
    // 首先确保区域内部连通
    for (const [regionId, points] of this.regionNavPoints) {
      for (let i = 0; i < points.length; i++) {
        const point1 = points[i];
        const [x1, y1] = point1.split(',').map(Number);
        const connections = [];
        
        // 连接同区域内的点
        for (let j = 0; j < points.length; j++) {
          if (i === j) continue;
          const point2 = points[j];
          const [x2, y2] = point2.split(',').map(Number);
          const dist = Math.abs(x2 - x1) + Math.abs(y2 - y1);
          
          if (dist <= this.MAX_CONNECTION_DIST && this.canDirectlyConnect(x1, y1, x2, y2)) {
            connections.push(point2);
          }
        }
        
        // 连接相邻区域的点
        const nearbyPoints = this.navPointsArray
          .filter(p => !points.includes(p))
          .map(p => {
            const [x2, y2] = p.split(',').map(Number);
            return {
              point: p,
              dist: Math.abs(x2 - x1) + Math.abs(y2 - y1)
            };
          })
          .filter(c => c.dist <= this.MAX_CONNECTION_DIST)
          .sort((a, b) => a.dist - b.dist)
          .slice(0, this.MAX_CONNECTIONS - connections.length);

        for (const {point} of nearbyPoints) {
          const [x2, y2] = point.split(',').map(Number);
          if (this.canDirectlyConnect(x1, y1, x2, y2)) {
            connections.push(point);
          }
        }

        if (connections.length > 0) {
          this.navPointConnections.set(point1, connections);
          this.stats.preprocess.totalConnections += connections.length;
        }
      }
    }
    
    const connEnd = process.hrtime(connStart);
    this.stats.preprocess.connectionBuildTime = connEnd[0] * 1000 + connEnd[1] / 1000000;
  }

  areRegionsAdjacent(region1, region2) {
    const [x1, y1] = region1.split(',').map(Number);
    const [x2, y2] = region2.split(',').map(Number);
    return Math.abs(x1 - x2) <= 1 && Math.abs(y1 - y2) <= 1;
  }

  canDirectlyConnect(x1, y1, x2, y2) {
    const dx = Math.sign(x2 - x1);
    const dy = Math.sign(y2 - y1);
    const steps = Math.max(Math.abs(x2 - x1), Math.abs(y2 - y1));
    
    // 检查路径上的点
    let blocked = 0;
    for (let i = 1; i < steps; i++) {
      const checkX = x1 + dx * i;
      const checkY = y1 + dy * i;
      if (!this.isWalkable(checkX, checkY)) {
        blocked++;
        // 允许最多2个障碍点，这样可以绕过小障碍
        if (blocked > 2) return false;
      }
    }
    
    return true;
  }

  // 在两个导航点之间寻路
  findPathBetweenNavPoints(startX, startY, endX, endY) {
    // 重置寻路统计
    this.stats.pathfinding = {
      localPathNodesExplored: 0,
      navPathNodesExplored: 0,
      maxOpenSetSize: 0,
      segmentCount: 0,
      navPointSearchTime: 0,
      navPathTime: 0,
      localPathTime: 0
    };

    const startRegion = this.getRegionId(startX, startY);
    const endRegion = this.getRegionId(endX, endY);
    
    // 如果在同一区域，直接用A*
    if (startRegion === endRegion) {
      const localStart = process.hrtime();
      const path = this.findLocalPath(startX, startY, endX, endY);
      const localEnd = process.hrtime(localStart);
      this.stats.pathfinding.localPathTime += localEnd[0] * 1000 + localEnd[1] / 1000000;
      return path;
    }

    // 找到最近的导航点
    const navPointStart = process.hrtime();
    const startNavPoint = this.findNearestNavPoint(startX, startY);
    const endNavPoint = this.findNearestNavPoint(endX, endY);
    const navPointEnd = process.hrtime(navPointStart);
    this.stats.pathfinding.navPointSearchTime = navPointEnd[0] * 1000 + navPointEnd[1] / 1000000;

    // 用A*连接导航点
    const navPathStart = process.hrtime();
    const navPath = this.findNavPointPath(startNavPoint, endNavPoint);
    const navPathEnd = process.hrtime(navPathStart);
    this.stats.pathfinding.navPathTime = navPathEnd[0] * 1000 + navPathEnd[1] / 1000000;
    
    if (!navPath) return null;

    // 构建完整路径
    const fullPath = [];
    const localPathStart = process.hrtime();
    
    // 1. 起点到第一个导航点
    const firstLeg = this.findLocalPath(startX, startY, 
      parseInt(navPath[0].split(',')[0]), 
      parseInt(navPath[0].split(',')[1]));
    if (firstLeg) {
      fullPath.push(...firstLeg);
      this.stats.pathfinding.segmentCount++;
    }

    // 2. 导航点之间的路径
    for (let i = 0; i < navPath.length - 1; i++) {
      const [x1, y1] = navPath[i].split(',').map(Number);
      const [x2, y2] = navPath[i + 1].split(',').map(Number);
      const segment = this.findLocalPath(x1, y1, x2, y2);
      if (segment) {
        fullPath.push(...segment);
        this.stats.pathfinding.segmentCount++;
      }
    }

    // 3. 最后一个导航点到终点
    const lastLeg = this.findLocalPath(
      parseInt(navPath[navPath.length - 1].split(',')[0]),
      parseInt(navPath[navPath.length - 1].split(',')[1]),
      endX, endY);
    if (lastLeg) {
      fullPath.push(...lastLeg);
      this.stats.pathfinding.segmentCount++;
    }

    const localPathEnd = process.hrtime(localPathStart);
    this.stats.pathfinding.localPathTime = localPathEnd[0] * 1000 + localPathEnd[1] / 1000000;

    return fullPath;
  }

  // 找最近的导航点
  findNearestNavPoint(x, y) {
    const regionId = this.getRegionId(x, y);
    const regionNavPoints = this.regionNavPoints.get(regionId);
    
    if (!regionNavPoints || regionNavPoints.length === 0) {
      // 如果当前区域没有导航点，检查相邻区域
      let nearest = null;
      let minDist = Infinity;
      
      for (const point of this.navPointsArray) {
        const [nx, ny] = point.split(',').map(Number);
        const dist = Math.abs(nx - x) + Math.abs(ny - y);
        if (dist < minDist) {
          minDist = dist;
          nearest = point;
        }
      }
      
      return nearest;
    }
    
    // 在当前区域内找最近的导航点
    let nearest = regionNavPoints[0];
    let minDist = Infinity;
    
    for (const point of regionNavPoints) {
      const [nx, ny] = point.split(',').map(Number);
      const dist = Math.abs(nx - x) + Math.abs(ny - y);
      if (dist < minDist) {
        minDist = dist;
        nearest = point;
      }
    }
    
    return nearest;
  }

  // 在导航点之间寻路
  findNavPointPath(start, end) {
    const openSet = new Set([start]);
    const cameFrom = new Map();
    const gScore = new Map([[start, 0]]);
    const fScore = new Map([[start, this.heuristic(start, end)]]);

    while (openSet.size > 0) {
      this.stats.pathfinding.navPathNodesExplored++;
      this.stats.pathfinding.maxOpenSetSize = Math.max(
        this.stats.pathfinding.maxOpenSetSize, 
        openSet.size
      );

      const current = Array.from(openSet).reduce((a, b) => 
        fScore.get(a) < fScore.get(b) ? a : b
      );

      if (current === end) {
        return this.reconstructPath(cameFrom, current);
      }

      openSet.delete(current);
      
      // 使用预处理的连接关系
      const neighbors = this.navPointConnections.get(current) || [];
      for (const neighbor of neighbors) {
        const tentativeGScore = gScore.get(current) + 
          this.heuristic(current, neighbor);

        if (!gScore.has(neighbor) || 
            tentativeGScore < gScore.get(neighbor)) {
          cameFrom.set(neighbor, current);
          gScore.set(neighbor, tentativeGScore);
          fScore.set(neighbor, tentativeGScore + 
            this.heuristic(neighbor, end));
          openSet.add(neighbor);
        }
      }
    }

    return null;
  }

  // 局部A*寻路
  findLocalPath(startX, startY, endX, endY) {
    // 使用简单的A*算法在小范围内寻路
    const openSet = new Set([`${startX},${startY}`]);
    const cameFrom = new Map();
    const gScore = new Map([[`${startX},${startY}`, 0]]);
    const fScore = new Map([[`${startX},${startY}`, 
      this.heuristic(`${startX},${startY}`, `${endX},${endY}`)]]);

    while (openSet.size > 0) {
      this.stats.pathfinding.localPathNodesExplored++;
      this.stats.pathfinding.maxOpenSetSize = Math.max(
        this.stats.pathfinding.maxOpenSetSize, 
        openSet.size
      );

      const current = Array.from(openSet).reduce((a, b) => 
        fScore.get(a) < fScore.get(b) ? a : b
      );

      if (current === `${endX},${endY}`) {
        return this.reconstructPath(cameFrom, current);
      }

      openSet.delete(current);
      const [x, y] = current.split(',').map(Number);

      for (const dir of DIRECTIONS) {
        const nx = x + dir.x * 2; // 使用2步移动
        const ny = y + dir.y * 2;
        
        if (!this.isWalkable(nx, ny)) continue;
        
        const neighbor = `${nx},${ny}`;
        const tentativeGScore = gScore.get(current) + 2;

        if (!gScore.has(neighbor) || 
            tentativeGScore < gScore.get(neighbor)) {
          cameFrom.set(neighbor, current);
          gScore.set(neighbor, tentativeGScore);
          fScore.set(neighbor, tentativeGScore + 
            this.heuristic(neighbor, `${endX},${endY}`));
          openSet.add(neighbor);
        }
      }
    }

    return null;
  }

  // 启发式函数
  heuristic(a, b) {
    const [x1, y1] = a.split(',').map(Number);
    const [x2, y2] = b.split(',').map(Number);
    return Math.abs(x1 - x2) + Math.abs(y1 - y2);
  }

  // 重建路径
  reconstructPath(cameFrom, current) {
    const path = [current];
    while (cameFrom.has(current)) {
      current = cameFrom.get(current);
      path.unshift(current);
    }
    return path;
  }
}

// 执行解析并输出结果
const mapcFilePath = path.join(
  __dirname,
  './mapc-out', '0.mapc'
);

// 测试寻路
parseMapcFile(mapcFilePath)
  .then(result => {
    console.log('地图数据解析成功:');
    console.log(`宽: ${result.width}, 高: ${result.height}`);
    console.log(`总单元格数: ${result.totalCells}, 文件大小: ${result.fileSize}字节`);

    // 创建导航系统
    console.log('\n开始预处理地图...');
    const startPreprocessTime = process.hrtime();
    const navSystem = new NavigationSystem(result.obstacleData);
    const endPreprocessTime = process.hrtime(startPreprocessTime);
    const preprocessTimeMs = (endPreprocessTime[0] * 1000 + endPreprocessTime[1] / 1000000).toFixed(2);

    // 输出预处理统计
    console.log('\n预处理性能统计:');
    console.log(`- 总耗时: ${preprocessTimeMs}ms`);
    console.log(`- 区域划分耗时: ${navSystem.stats.preprocess.regionDivisionTime.toFixed(2)}ms`);
    console.log(`- 导航点检测耗时: ${navSystem.stats.preprocess.navPointDetectionTime.toFixed(2)}ms`);
    console.log(`- 连接建立耗时: ${navSystem.stats.preprocess.connectionBuildTime.toFixed(2)}ms`);
    console.log('\n预处理结果:');
    console.log(`- 总节点数: ${navSystem.stats.preprocess.totalNodes}`);
    console.log(`- 区域数量: ${navSystem.stats.preprocess.totalRegions}`);
    console.log(`- 导航点数量: ${navSystem.stats.preprocess.totalNavPoints}`);
    console.log(`- 区域连接数: ${navSystem.stats.preprocess.totalConnections}`);

    // 起点和终点
    const startX = 630;
    const startY = 611;
    const targetX = 100;
    const targetY = 200;

    // 执行寻路
    console.log('\n开始寻路...');
    const startPathTime = process.hrtime();
    const path = navSystem.findPathBetweenNavPoints(startX, startY, targetX, targetY);
    const endPathTime = process.hrtime(startPathTime);
    const pathTimeMs = (endPathTime[0] * 1000 + endPathTime[1] / 1000000).toFixed(2);

    if (path) {
      console.log(`\n寻路成功! 总耗时: ${pathTimeMs}ms`);
      console.log('\n寻路性能统计:');
      console.log(`- 导航点查找耗时: ${navSystem.stats.pathfinding.navPointSearchTime.toFixed(2)}ms`);
      console.log(`- 导航点寻路耗时: ${navSystem.stats.pathfinding.navPathTime.toFixed(2)}ms`);
      console.log(`- 局部路径耗时: ${navSystem.stats.pathfinding.localPathTime.toFixed(2)}ms`);
      console.log(`- 探索导航节点数: ${navSystem.stats.pathfinding.navPathNodesExplored}`);
      console.log(`- 探索局部节点数: ${navSystem.stats.pathfinding.localPathNodesExplored}`);
      console.log(`- 最大开放列表大小: ${navSystem.stats.pathfinding.maxOpenSetSize}`);
      console.log(`- 路径段数: ${navSystem.stats.pathfinding.segmentCount}`);
      
      console.log('\n路径信息:');
      console.log(`- 起点: (${startX}, ${startY})`);
      console.log(`- 终点: (${targetX}, ${targetY})`);
      console.log(`- 路径长度: ${path.length} 步`);
      
      console.log('\n路径前10步:');
      path.slice(0, 10).forEach((point, index) => {
        console.log(`步骤 ${index + 1}: ${point}`);
      });
      if (path.length > 10) {
        console.log('...');
      }
    } else {
      console.log(`\n未找到可行路径! 耗时: ${pathTimeMs}ms`);
      console.log(`起点: (${startX}, ${startY})`);
      console.log(`终点: (${targetX}, ${targetY})`);
    }
  })
  .catch(error => {
    console.error('解析失败:', error.message);
  });