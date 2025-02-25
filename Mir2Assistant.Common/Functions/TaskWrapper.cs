using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mir2Assistant.Common.Utils;

namespace Mir2Assistant.Common.Functions
{
    public static class TaskWrapper
    {
        /// <summary>
        /// 等待任务完成
        /// </summary>
        /// <param name="ack">确认是否完成</param>
        /// <param name="timeout">超时*100毫秒</param>
        /// <returns>是否成功</returns>
        public static async Task<bool> Wait(Func<bool> ack, int timeout = 10)
        {
            var success = true;
            await Task.Run(() =>
            {
                int i = 0;
                while (!ack())
                {
                    Task.Delay(100).Wait();
                    i++;
                    if (i > timeout)
                    {
                        success = false;
                        return;
                    }
                }
            });
            return success;
        }
    }
}
