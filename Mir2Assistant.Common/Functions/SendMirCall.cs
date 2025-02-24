using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
                    SendMessage(gameInstance.MirHwnd, 0x4a, 20250129, ref cds);
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
                    SendMessage(gameInstance.MirHwnd, 0x4A, 20250129, ref cds);
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
