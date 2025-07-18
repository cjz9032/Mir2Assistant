using System.Collections.Generic;
using System;

namespace Mir2Assistant.Common.Utils
{
    public static class StringUtils
    {
        /// <summary>
        /// 生成紧凑字符串数据
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>生成的紧凑字符串数据</returns>
        public static nint[] GenerateCompactStringData(string input)
        {
            char[] memberChars = input.ToCharArray(); 
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
    }
}