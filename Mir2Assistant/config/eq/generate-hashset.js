const fs = require('fs');

// 读取 JSON 文件
const normalItems = require('./eq-normal.json');
const jpItems = require('./eq-jp.json');

// 合并所有物品
const allItems = [...normalItems, ...jpItems];


// 生成 C# HashSet 代码
function generateHashSet(items) {
    let csharpCode = `
using System;
using System.Collections.Generic;

namespace Mir2Assistant.Common.Constants
{
    public static class ItemDatabase
    {
        // Name -> [Ac, Ac2, Mac, Mac2, Dc, Dc2, Mc, Mc2, Sc, Sc2]
        public static readonly Dictionary<string, byte[]> ItemStats = new Dictionary<string, byte[]>
        {`;

    items.forEach(item => {
        const stats = [
            item.Ac || 0,
            item.Ac2 || 0,
            item.Mac || 0,
            item.Mac2 || 0,
            item.Dc || 0,
            item.Dc2 || 0,
            item.Mc || 0,
            item.Mc2 || 0,
            item.Sc || 0,
            item.Sc2 || 0
        ];

        const statsArray = stats.map(s => s.toString()).join(', ');
        csharpCode += `\n            { "${item.Name}", new byte[] { ${statsArray} } },`;
    });

    csharpCode += `
        };

        // JP items
        public static readonly HashSet<short> JpItemsByLooks = new HashSet<short>
        {`;
    jpItems.forEach(item => {
        csharpCode += `\n            ${item.Looks},`;
    });
    csharpCode += `\n        };`;


    csharpCode += `
        
        public static bool IsValidItem(string itemName)
        {
            return ItemStats.ContainsKey(itemName);
        }
        
        public static byte[] GetItemStats(string itemName)
        {
            return ItemStats.TryGetValue(itemName, out byte[] stats) ? stats : null;
        }
    }
}`;

    return csharpCode;
}

// 生成代码
const generatedCode = generateHashSet(allItems);

// 写入文件
fs.writeFileSync('../ItemDatabase.cs', generatedCode);

console.log(`Generated ItemDatabase.cs with ${allItems.length} items`);
console.log('Usage example:');
console.log('bool exists = ItemDatabase.IsValidItem("布衣(男)");');
console.log('byte[] stats = ItemDatabase.GetItemStats("布衣(男)");');
