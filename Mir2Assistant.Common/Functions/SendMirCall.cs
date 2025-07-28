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

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(int dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        static extern int CloseHandle(IntPtr hObject);

        const int THREAD_SUSPEND_RESUME = 0x0002;

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
            public CancellationToken CancellationToken;
            public int ThreadId;
        }

        private static void SendMessageThreadProc(object state)
        {
            var parameters = (SendMessageParams)state;
            parameters.ThreadId = Thread.CurrentThread.ManagedThreadId;
            
            if (parameters.CancellationToken.IsCancellationRequested)
            {
                return;
            }
            SendMessage(parameters.Hwnd, parameters.Msg, parameters.WParam, ref parameters.LParam);
        }

        private static void SendMessageWithTimeout(nint hwnd, uint msg, IntPtr wParam, ref COPYDATASTRUCT lParam)
        {
            using (var cts = new CancellationTokenSource())
            {
                var parameters = new SendMessageParams
                {
                    Hwnd = hwnd,
                    Msg = msg,
                    WParam = wParam,
                    LParam = lParam,
                    CancellationToken = cts.Token
                };

                var thread = new Thread(SendMessageThreadProc);
                thread.Start(parameters);

                if (!thread.Join(1000))
                {
                    cts.Cancel();
                    
                    // 获取线程句柄并挂起它
                    IntPtr threadHandle = OpenThread(THREAD_SUSPEND_RESUME, false, (uint)parameters.ThreadId);
                    if (threadHandle != IntPtr.Zero)
                    {
                        try
                        {
                            SuspendThread(threadHandle);
                        }
                        finally
                        {
                            CloseHandle(threadHandle);
                        }
                    }
                    
                    throw new TimeoutException("SendMessage operation timed out");
                }

                // 更新原始的lParam，以防消息处理修改了它
                lParam = parameters.LParam;
            }
        }

        // public static void Send(MirGameInstanceModel gameInstance, nint code, byte[] data)
        // {
        //     int size = data.Length * sizeof(byte);
        //     IntPtr unmanagedPointer = Marshal.AllocHGlobal(size);

        //     COPYDATASTRUCT cds;
        //     cds.dwData = (uint)code; // 自定义数据，可以是任何值
        //     cds.cbData = size;
        //     cds.lpData = unmanagedPointer;

        //     try
        //     {
        //         Marshal.Copy(data, 0, unmanagedPointer, data.Length);
        //         SendMessageWithTimeout(gameInstance.MirHwnd, 0x4a, 20250129, ref cds);
        //     }
        //     catch (TimeoutException)
        //     {
        //     }
        //     finally
        //     {
        //         Marshal.FreeHGlobal(unmanagedPointer);
        //     }
        // }

        public static void Send(MirGameInstanceModel gameInstance, nint code, nint[] data)
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
                gameInstance.GameDebug($"发送消息超时 code {code} 数据 {string.Join(",", data)}");
                // 如果发送超时，这里可以处理，比如重试或者记录日志
            }
            finally
            {
                Marshal.FreeHGlobal(unmanagedPointer);
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
