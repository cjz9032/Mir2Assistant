using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mir2Assistant.Common.Utils;

public class HotKeyUtils
{
    [System.Runtime.InteropServices.DllImport("user32.dll")] //导入WinAPI 

    public static extern bool RegisterHotKey( //设置热键
        IntPtr hWnd, // 窗口句柄，一般使用Handle属性 
        int id, // 区别热键的ID号,这个可以随便写，只是用来区分不同热键 
        uint fsModifiers, // 修正键用户接下哪些键是发生 可能为contol=2， alt=1， shift=4， windows=8或这些键的组合，如果没有的话直接用0 
        Keys vk // 键 
    );

    [System.Runtime.InteropServices.DllImport("user32.dll")] //导入WinAPI 
    public static extern bool UnregisterHotKey( //注销热键 
        IntPtr hWnd, // 窗口句柄
        int id // 键标识 
    );
}