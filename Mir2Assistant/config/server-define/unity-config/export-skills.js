const fs = require('fs');
const path = require('path');

// 读取 CSV 文件
const csvFilePath = path.join(__dirname, 'magic.csv');
const csvContent = fs.readFileSync(csvFilePath, 'utf-8');

// 解析 CSV
const lines = csvContent.trim().split('\n');
const headers = lines[0].split(',');

const skills = [];

for (let i = 1; i < lines.length; i++) {
    const values = lines[i].split(',');
    const skill = {};
    
    headers.forEach((header, index) => {
        const value = values[index];
        // 尝试转换为数字
        if (value && !isNaN(value) && value.trim() !== '') {
            skill[header] = Number(value);
        } else {
            skill[header] = value || '';
        }
    });
    
    skills.push(skill);
}

// 创建输出对象
const output = {
    skills: skills,
    skillsById: {},
    skillsByName: {}
};

// 按 ID 和名称索引
skills.forEach(skill => {
    output.skillsById[skill.MagID] = skill;
    output.skillsByName[skill.MagName] = skill;
});

// 导出 JSON
const outputPath = path.join(__dirname, 'skills.json');
fs.writeFileSync(outputPath, JSON.stringify(output, null, 2), 'utf-8');

// 生成 C# MagicSpellMap 代码
const generateCSharpSpellMap = () => {
    const spellMapEntries = skills.map(skill => {
        return `{ ${skill.MagID}, ${skill.Spell} }`;
    });
    
    // 每行最多9个条目
    const lines = [];
    for (let i = 0; i < spellMapEntries.length; i += 9) {
        lines.push('        ' + spellMapEntries.slice(i, i + 9).join(', '));
    }
    
    return `    public static readonly Dictionary<int, int> MagicSpellMap = new()
    {
${lines.join(',\n')}
    };`;
};

// 生成 C# DefSpellMap 代码
const generateCSharpDefSpellMap = () => {
    const defSpellMapEntries = skills.map(skill => {
        return `{ ${skill.MagID}, ${skill.DefSpell} }`;
    });
    
    // 每行最多9个条目
    const lines = [];
    for (let i = 0; i < defSpellMapEntries.length; i += 9) {
        lines.push('        ' + defSpellMapEntries.slice(i, i + 9).join(', '));
    }
    
    return `    public static readonly Dictionary<int, int> MagicDefSpellMap = new()
    {
${lines.join(',\n')}
    };`;
};

// 输出 C# 代码
const csharpCode = `// 自动生成的技能消耗MP映射
${generateCSharpSpellMap()}

// 自动生成的技能防御消耗MP映射
${generateCSharpDefSpellMap()}
`;

const csharpOutputPath = path.join(__dirname, 'MagicSpellMap.cs');
fs.writeFileSync(csharpOutputPath, csharpCode, 'utf-8');

console.log(`✅ 成功导出 ${skills.length} 个技能到 ${outputPath}`);
console.log(`✅ 成功生成 C# 代码到 ${csharpOutputPath}`);
console.log(`\n技能列表：`);
skills.forEach(skill => {
    console.log(`  - [${skill.MagID}] ${skill.MagName} (消耗MP: ${skill.Spell}, 防御消耗: ${skill.DefSpell}, 延迟: ${skill.Delay}ms)`);
});
