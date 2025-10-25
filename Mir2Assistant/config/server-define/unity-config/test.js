var map = require('./map/D2002.json');
// console.log(map.c);

// 获取指定x,y的值
var x = 34;
var y = 39;
console.log(map.c[x + y * map.width]);
