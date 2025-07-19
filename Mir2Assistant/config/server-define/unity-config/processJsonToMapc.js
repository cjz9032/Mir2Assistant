const fs = require('fs');
const path = require('path');

// 定义要处理的文件夹路径
const jsonFolderPath = path.join(__dirname, 'map');
const outFolderPath = path.join(__dirname, 'mapc-out');

// 读取文件夹中的所有文件
fs.readdir(jsonFolderPath, (err, files) => {
    if (err) {
        console.error('读取文件夹失败:', err);
        return;
    }

    // 过滤出所有的 JSON 文件
    const jsonFiles = files.filter(file => path.extname(file) === '.json');

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

            // 创建二进制缓冲区
            const buffer = Buffer.alloc(8 + obstacles.length);

            // 写入宽高
            buffer.writeInt32LE(width, 0);
            buffer.writeInt32LE(height, 4);

            // 写入障碍物标志
            obstacles.forEach((value, index) => {
                buffer.writeUInt8(value, 8 + index);
            });

            // 生成输出文件路径
            const outputFileName = path.basename(jsonFile, '.json') + '.mapc';
            const outputFilePath = path.join(outFolderPath, outputFileName);

            // 保存二进制文件
            fs.writeFileSync(outputFilePath, buffer);
            console.log(`成功处理 ${jsonFile}，生成 ${outputFileName}`);
        } catch (error) {
            console.error(`处理 ${jsonFile} 时出错:`, error);
        }
    });
});