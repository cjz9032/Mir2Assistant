using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mir2Assistant
{
    public class DllInject
    {

        public enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }


        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(HookType hookType, IntPtr lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern int FindWindow(string sClass, string sWindow);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(uint hHandle, uint dwMilliseconds);



        public static void SetHook(IntPtr addr, MirGameInstanceModel gi)
        {
            var handle = SetWindowsHookEx(HookType.WH_CALLWNDPROC, addr, gi.LibIpdl!.Value, gi.MirThreadId);

            if (handle == IntPtr.Zero)
            {
                MessageBox.Show("Injection failed to set hook.");
                return;
            }
            gi.HookHandle = handle;
            while (gi.LibIpdl > 0)
            {
                Thread.Sleep(100);
            }
        }

        public static void Unhook(MirGameInstanceModel gi)
        {
            SendMirCall.Send(gi, 9002, new nint[] { });
            if (gi.HookHandle != null && gi.HookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(gi.HookHandle!.Value);
                FreeLibrary(gi.LibIpdl!.Value);
                gi.LibIpdl = 0;
            }
        }

        public static void loadDll(MirGameInstanceModel gi)
        {
            IntPtr addr = (IntPtr)0;
            string dllPath = Path.Combine(Application.StartupPath, "Mir2HookCall.dll");
            
            if (!File.Exists(dllPath))
            {
                string publishPath = Path.Combine(Application.StartupPath, "publish", "Mir2HookCall.dll");
                if (File.Exists(publishPath))
                {
                    dllPath = publishPath;
                }
            }
            
            var ipdl = LoadLibrary(dllPath);

            if (ipdl == IntPtr.Zero)
            {
                MessageBox.Show("Injection failed: could not load target DLL.");
                return;
            }
            
            addr = GetProcAddress(ipdl, "HookProc");
            if (addr == IntPtr.Zero)
            {
                MessageBox.Show("Injection failed: could not DLL entrypoint (did you read the documentation?).");
                return;
            }
            gi.LibIpdl = ipdl;
            var thread = new Thread(() => SetHook(addr, gi));
            thread.Start();
            
            
            // 等待DLL加载完成
            //await Task.Delay(5000);

            // 使用SendMirCall发送账号信息
             if (gi.AccountInfo != null && !string.IsNullOrEmpty(gi.AccountInfo.Account))
            {
                // 将账号和密码转换为字符数组
                char[] accountChars = gi.AccountInfo.Account.ToCharArray();
                char[] passwordChars = gi.AccountInfo.Password.ToCharArray();

                // 创建一个足够大的数组来存储所有数据
                // 格式: [账号长度, 密码长度, 账号字符1, 账号字符2, ..., 密码字符1, 密码字符2, ...]
                nint[] data = new nint[2 + accountChars.Length + passwordChars.Length];

                // 存储长度信息
                data[0] = accountChars.Length;
                data[1] = passwordChars.Length;

                // 存储账号字符
                for (int i = 0; i < accountChars.Length; i++)
                {
                    data[2 + i] = accountChars[i];
                }

                // 存储密码字符
                for (int i = 0; i < passwordChars.Length; i++)
                {
                    data[2 + accountChars.Length + i] = passwordChars[i];
                }

                // 一次性发送所有数据
                SendMirCall.Send(gi, 9003, data);
            }
        }
    }
}
