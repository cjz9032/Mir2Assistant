﻿using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;
using Mir2Assistant.Common.Utils;
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

        async public static void loadDll(MirGameInstanceModel gi)
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
            await Task.Delay(4000);

            if (gi.AccountInfo != null && !string.IsNullOrEmpty(gi.AccountInfo.Account))
            {
                nint[] data = MemoryUtils.PackStringsToData(gi.AccountInfo.Account, gi.AccountInfo.Password);
                SendMirCall.Send(gi, 9003, data);
            }
        }

        // 将字符串数组打包成nint数组用于传递给DLL
        public static nint[] PackStringsToData(params string[] strings)
        {
            if (strings == null || strings.Length == 0)
                return new nint[0];

            // 计算所有字符的总数
            int totalChars = 0;
            foreach (var str in strings)
            {
                totalChars += str?.Length ?? 0;
            }

            // 创建数据数组：前N个元素存储每个字符串的长度，后面存储所有字符
            nint[] data = new nint[strings.Length + totalChars];

            // 存储长度信息
            int dataIndex = 0;
            for (int i = 0; i < strings.Length; i++)
            {
                int length = strings[i]?.Length ?? 0;
                data[dataIndex++] = length;
            }

            // 存储字符数据
            for (int i = 0; i < strings.Length; i++)
            {
                if (string.IsNullOrEmpty(strings[i]))
                    continue;

                char[] chars = strings[i].ToCharArray();
                for (int j = 0; j < chars.Length; j++)
                {
                    data[dataIndex++] = chars[j];
                }
            }

            return data;
        }
    }
}