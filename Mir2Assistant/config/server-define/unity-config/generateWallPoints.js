const fs = require('fs');
const path = require('path');
const MIN_SAFE_PTS = 3;
// 定义8个方向的偏移量 (x, y)
const directions = [
    [-1, -1], [0, -1], [1, -1],  // 上排：左上、上、右上
    [-1,  0],          [1,  0],  // 中排：左、右
    [-1,  1], [0,  1], [1,  1]   // 下排：左下、下、右下
];

// 网格大小（用于空间索引）
const GRID_SIZE = 50;

// 检查指定位置是否为障碍物
function isObstacle(obstacles, width, height, x, y) {
    if (x < 0 || x >= width || y < 0 || y >= height) {
        return true; // 地图边界视为障碍物
    }
    const index = y * width + x;
    return obstacles[index] !== 0; // 假设0表示可通行，非0表示障碍物
}

// 计算指定位置的靠墙数（8个方向中障碍物的数量）
function getWallCount(obstacles, width, height, x, y) {
    // 首先检查当前位置是否可通行
    if (isObstacle(obstacles, width, height, x, y)) {
        return 0; // 障碍物位置不能作为靠墙点
    }
    
    let obstacleCount = 0;
    for (const [dx, dy] of directions) {
        if (isObstacle(obstacles, width, height, x + dx, y + dy)) {
            obstacleCount++;
        }
    }
    
    return obstacleCount;
}

// 检查指定位置是否为靠墙点（8个方向中至少3个是障碍物）
function isWallPoint(obstacles, width, height, x, y) {
    return getWallCount(obstacles, width, height, x, y) >= MIN_SAFE_PTS;
}

// 生成靠墙点列表并按网格组织
function generateWallPointsWithGrid(obstacles, width, height) {
    const wallPoints = [];
    
    // 计算网格尺寸
    const gridWidth = Math.ceil(width / GRID_SIZE);
    const gridHeight = Math.ceil(height / GRID_SIZE);
    
    // 初始化二维网格数组
    const grids = [];
    for (let gy = 0; gy < gridHeight; gy++) {
        grids[gy] = [];
        for (let gx = 0; gx < gridWidth; gx++) {
            grids[gy][gx] = [];
        }
    }
    
    // 遍历地图找靠墙点
    for (let y = 0; y < height; y++) {
        for (let x = 0; x < width; x++) {
            const wallCount = getWallCount(obstacles, width, height, x, y);
            if (wallCount >= MIN_SAFE_PTS) {
                const point = { x, y, wallCount };
                wallPoints.push(point);
                
                // 计算网格坐标并添加到对应网格
                const gridX = Math.floor(x / GRID_SIZE);
                const gridY = Math.floor(y / GRID_SIZE);
                grids[gridY][gridX].push(point);
            }
        }
    }
    
    return {
        wallPoints,
        grids,
        gridWidth,
        gridHeight
    };
}

// 定义要处理的文件夹路径
const jsonFolderPath = path.join(__dirname, 'map');
const outFolderPath = path.join(__dirname, 'wallpoints-out');

// 确保输出文件夹存在
if (!fs.existsSync(outFolderPath)) {
    fs.mkdirSync(outFolderPath, { recursive: true });
}

// 读取文件夹中的所有文件
fs.readdir(jsonFolderPath, (err, files) => {
    if (err) {
        console.error('读取文件夹失败:', err);
        return;
    }

    // 过滤出所有的 JSON 文件
    const jsonFiles = files.filter(file => path.extname(file) === '.json');
    
    console.log(`找到 ${jsonFiles.length} 个地图文件`);
    console.log(`网格大小: ${GRID_SIZE}x${GRID_SIZE}`);

    // 处理每个 JSON 文件
    jsonFiles.forEach(jsonFile => {
        const jsonFilePath = path.join(jsonFolderPath, jsonFile);
        try {
            // 读取 JSON 文件
            const jsonData = JSON.parse(fs.readFileSync(jsonFilePath, 'utf8'));

            // 提取宽高和障碍物标志
            const width = jsonData.width;
            const height = jsonData.height;
            const obstacles = jsonData.c;

            console.log(`处理地图 ${jsonFile}: ${width}x${height}`);

            // 生成靠墙点和网格索引
            const { wallPoints, grids, gridWidth, gridHeight } = generateWallPointsWithGrid(obstacles, width, height);
            
            console.log(`  找到 ${wallPoints.length} 个靠墙点`);
            console.log(`  网格尺寸: ${gridWidth}x${gridHeight}`);

            // 统计非空网格数量
            let nonEmptyGrids = 0;
            let totalPointsInGrids = 0;
            for (let gy = 0; gy < gridHeight; gy++) {
                for (let gx = 0; gx < gridWidth; gx++) {
                    if (grids[gy][gx].length > 0) {
                        nonEmptyGrids++;
                        totalPointsInGrids += grids[gy][gx].length;
                    }
                }
            }
            console.log(`  非空网格: ${nonEmptyGrids}/${gridWidth * gridHeight}`);

            // 创建优化后的输出数据
            const Data = {
                mapName: path.basename(jsonFile, '.json'),
                width: width,
                height: height,
                gridSize: GRID_SIZE,
                gridWidth: gridWidth,
                gridHeight: gridHeight,
                wallPointsCount: wallPoints.length,
                nonEmptyGridsCount: nonEmptyGrids,
                grids: grids
            };

            // 生成JSON输出文件
            const jsonOutputFileName = path.basename(jsonFile, '.json') + '_wallpoints.json';
            const jsonOutputFilePath = path.join(outFolderPath, jsonOutputFileName);
            fs.writeFileSync(jsonOutputFilePath, JSON.stringify(Data, null, 2));

            // 生成二进制输出文件（优化版本）
            const binaryOutputFileName = path.basename(jsonFile, '.json') + '.wallpoints';
            const binaryOutputFilePath = path.join(outFolderPath, binaryOutputFileName);
            
            // 二进制格式：
            // [width(4)][height(4)][gridSize(4)][gridWidth(4)][gridHeight(4)][totalPoints(4)]
            // [gridData: 对每个网格 -> pointCount(4) + points...]
            // 每个点: [x(4)][y(4)][wallCount(4)]
            let totalSize = 24; // 头部6个int32
            
            // 计算所需缓冲区大小
            for (let gy = 0; gy < gridHeight; gy++) {
                for (let gx = 0; gx < gridWidth; gx++) {
                    totalSize += 4; // pointCount
                    totalSize += grids[gy][gx].length * 12; // 每个点12字节(x,y,wallCount)
                }
            }
            
            const buffer = Buffer.alloc(totalSize);
            let offset = 0;
            
            // 写入头部信息
            buffer.writeInt32LE(width, offset); offset += 4;
            buffer.writeInt32LE(height, offset); offset += 4;
            buffer.writeInt32LE(GRID_SIZE, offset); offset += 4;
            buffer.writeInt32LE(gridWidth, offset); offset += 4;
            buffer.writeInt32LE(gridHeight, offset); offset += 4;
            buffer.writeInt32LE(wallPoints.length, offset); offset += 4;
            
            // 写入网格数据
            for (let gy = 0; gy < gridHeight; gy++) {
                for (let gx = 0; gx < gridWidth; gx++) {
                    const gridPoints = grids[gy][gx];
                    buffer.writeInt32LE(gridPoints.length, offset); offset += 4;
                    
                    gridPoints.forEach(point => {
                        buffer.writeInt32LE(point.x, offset); offset += 4;
                        buffer.writeInt32LE(point.y, offset); offset += 4;
                        buffer.writeInt32LE(point.wallCount, offset); offset += 4;
                    });
                }
            }
            
            fs.writeFileSync(binaryOutputFilePath, buffer);

            console.log(`  生成文件: ${jsonOutputFileName}, ${binaryOutputFileName}`);
            
        } catch (error) {
            console.error(`处理 ${jsonFile} 时出错:`, error);
        }
    });
    
    console.log('所有地图处理完成！');
    console.log(`优化效果: 从O(n)线性搜索优化为O(k)网格搜索，k通常远小于n`);
});
