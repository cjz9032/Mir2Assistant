using System.Collections.Generic;
using System;

namespace Mir2Assistant.Common.Utils
{
    public static class StringUtils
    {
        /// <summary>
        /// 生成紧凑的字符串数据
        /// </summary>
        public static nint[] GenerateCompactStringData(string str)
        {
            char[] memberChars = str.ToCharArray(); 
            nint[] data = new nint[1 + memberChars.Length]; 
 
            // 第一个元素存长度 
            data[0] = memberChars.Length; 
            // 后面存字符 
            for (int i = 0; i < memberChars.Length; i++) 
            { 
                data[1 + i] = memberChars[i]; 
            } 
            return data;
        }

        /// <summary>
        /// 构造混合数据数组，支持字符串和数值的组合
        /// </summary>
        /// <param name="values">参数值（支持string和int/nint类型）</param>
        /// <returns>构造好的nint数组</returns>
        public static nint[] GenerateMixedData(params object[] values)
        {
            List<nint> result = new List<nint>();
            
            foreach (var value in values)
            {
                if (value is string strValue)
                {
                    result.AddRange(GenerateCompactStringData(strValue));
                }
                else if (value is int intValue)
                {
                    result.Add(intValue);
                }
                else if (value is nint nintValue)
                {
                    result.Add(nintValue);
                }
                else if (value is byte btValue)
                {
                    result.Add(btValue);
                }
            }

            return result.ToArray();
        }
    }
}