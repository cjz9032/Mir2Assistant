using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mir2Assistant.Common.Functions
{
    public class SendMirCall
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SendMessage(nint hwnd, uint msg, IntPtr wParam, ref COPYDATASTRUCT lParam);

        [StructLayout(LayoutKind.Sequential)]
        struct COPYDATASTRUCT
        {
            public uint dwData;
            public int cbData;
            public IntPtr lpData;
        }

        private class SendMessageParams
        {
            public nint Hwnd;
            public uint Msg;
            public IntPtr WParam;
            public COPYDATASTRUCT LParam;
        }

        private static void SendMessageThreadProc(object state)
        {
            var parameters = (SendMessageParams)state;
            SendMessage(parameters.Hwnd, parameters.Msg, parameters.WParam, ref parameters.LParam);
        }

        private static void SendMessageWithTimeout(nint hwnd, uint msg, IntPtr wParam, ref COPYDATASTRUCT lParam)
        {
            var parameters = new SendMessageParams
            {
                Hwnd = hwnd,
                Msg = msg,
                WParam = wParam,
                LParam = lParam
            };

            var thread = new Thread(SendMessageThreadProc);
            thread.Start(parameters);

            // 如果3秒后线程还在运行，强制结束它
            if (!thread.Join(1000))
            {
                thread.Abort(); // 强制终止线程
                throw new TimeoutException("SendMessage operation timed out");
            }

            // 更新原始的lParam，以防消息处理修改了它
            lParam = parameters.LParam;
        }

        public static void Send(MirGameInstanceModel gameInstance, nint code, byte[] data)
        {
            lock (gameInstance)
            {
                int size = data.Length * sizeof(byte);
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(size);

                COPYDATASTRUCT cds;
                cds.dwData = (uint)code; // 自定义数据，可以是任何值
                cds.cbData = size;
                cds.lpData = unmanagedPointer;

                try
                {
                    Marshal.Copy(data, 0, unmanagedPointer, data.Length);
                    SendMessageWithTimeout(gameInstance.MirHwnd, 0x4a, 20250129, ref cds);
                }
                catch (TimeoutException)
                {
                    // 如果发送超时，这里可以处理，比如重试或者记录日志
                }
                finally
                {
                    Marshal.FreeHGlobal(unmanagedPointer);
                }
            }
        }

        public static void Send(MirGameInstanceModel gameInstance, nint code, nint[] data)
        {
            lock (gameInstance)
            {
                int size = data.Length * sizeof(int);
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(size);

                COPYDATASTRUCT cds;
                cds.dwData = (uint)code; // 自定义数据，可以是任何值
                cds.cbData = size;
                cds.lpData = unmanagedPointer;

                try
                {
                    Marshal.Copy(data, 0, unmanagedPointer, data.Length);
                    SendMessageWithTimeout(gameInstance.MirHwnd, 0x4A, 20250129, ref cds);
                }
                catch (TimeoutException)
                {
                    // 如果发送超时，这里可以处理，比如重试或者记录日志
                }
                finally
                {
                    Marshal.FreeHGlobal(unmanagedPointer);
                }
            }
        }

        public static nint[] String2NIntArray(string p)
        {
            var l0 = p.Length % 4 + 4;
            var bytes = Encoding.GetEncoding("gb2312").GetBytes(p + new string('\0', l0));
            return Enumerable.Range(0, bytes.Length / 4)
                           .Select(i => (nint)BitConverter.ToInt32(bytes, i * 4))
                           .ToArray();
        }
    }
}
