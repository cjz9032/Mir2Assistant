using Mir2Assistant.Common.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Mir2Assistant.Common.Utils;
public class MemoryUtils : IDisposable
{
    #region API

    //从指定内存中读取字节集数据
    [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, IntPtr lpNumberOfBytesRead);

    //从指定内存中写入字节集数据
    [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory")]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, IntPtr lpNumberOfBytesWritten);

    //打开一个已存在的进程对象，并返回进程的句柄
    [DllImport("kernel32.dll", EntryPoint = "OpenProcess")]
    private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    //关闭一个内核对象。其中包括文件、文件映射、进程、线程、安全和同步对象等。
    [DllImport("kernel32.dll")]
    private static extern void CloseHandle(IntPtr hObject);

    #endregion

    // byte		(字节   1字节)
    // int16    (short 2字节),
    // int32    (int   4字节),
    // int64    (long  8字节)
    // float    (单浮点 Single 4字节
    // double   (双浮点 8字节)
    // string	(字符串)
    // byte[]	(字节数组)

    private readonly MirGameInstanceModel _gameInstance;
    private nint _handle;
    private bool _disposed = false;

    public MemoryUtils(MirGameInstanceModel gameInstance)
    {
        _gameInstance = gameInstance;
        _handle = OpenProcess(0x1F0FFF, false, gameInstance.MirPid);
    }

    ~MemoryUtils()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // 释放托管资源
            }

            // 释放非托管资源
            if (_handle != IntPtr.Zero)
            {
                CloseHandle(_handle);
                _handle = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    #region Read


    public byte[] ReadToBytes(IntPtr address, int size)
    {
        lock (_gameInstance)
        {
            byte[] buffer = new byte[size];
            ReadProcessMemory(_handle, address, buffer, size, IntPtr.Zero);
            return buffer;
        }
    }
    public T ReadObject<T>(IntPtr address) where T : struct
    {
        var buffer = ReadToBytes(address, Marshal.SizeOf(typeof(T)));

        var bufferAddress = Marshal.AllocHGlobal(buffer.Length);

        Marshal.Copy(buffer, 0, bufferAddress, buffer.Length);

        var structure = (T)Marshal.PtrToStructure(bufferAddress, typeof(T))!;

        Marshal.FreeHGlobal(bufferAddress);

        return structure;
    }

    public char ReadToChar(IntPtr address)
    {
        byte[] buffer = ReadToBytes(address, sizeof(char));
        return BitConverter.ToChar(buffer, 0);
    }
    public short ReadToShort(IntPtr address)
    {
        byte[] buffer = ReadToBytes(address, sizeof(short));
        return BitConverter.ToInt16(buffer, 0);
    }
    public byte ReadToInt8(IntPtr address)
    {
        byte[] buffer = ReadToBytes(address, sizeof(byte));
        return buffer[0]; // 直接返回第一个字节
    }

    public int ReadToInt(IntPtr address)
    {
        byte[] buffer = ReadToBytes(address, sizeof(int));

        return BitConverter.ToInt32(buffer, 0);
    }

    public long ReadToLong(IntPtr address)
    {
        byte[] buffer = ReadToBytes(address, sizeof(long));
        return BitConverter.ToInt64(buffer, 0);
    }

    public float ReadToFloat(IntPtr address)
    {
        byte[] buffer = ReadToBytes(address, sizeof(float));
        return BitConverter.ToSingle(buffer, 0);
    }

    public double ReadToDouble(IntPtr address)
    {
        byte[] buffer = ReadToBytes(address, sizeof(double));
        return BitConverter.ToDouble(buffer, 0);
    }

    public string ReadToString(IntPtr address, int stringSize = 20)
    {
        byte[] buffer = ReadToBytes(address, stringSize);
        string asciiString = Encoding.GetEncoding("gb2312").GetString(buffer).Split('\0')[0].TrimEnd(); // 获取ASCII字符串并去除结尾的空字符
        return asciiString;
    }

    public string ReadToUnicode(IntPtr address, int stringSize = 20)
    {
        // data likes 71 00 77 00 65 00 61 00 31 00 33 00 
        byte[] buffer = ReadToBytes(address, stringSize);
        return Encoding.Unicode.GetString(buffer);
    }

    public string ReadToDelphiUnicode(IntPtr address, int rate = 2)
    {
        var len = ReadToInt(address - 4) * rate;
        return ReadToUnicode(address, len);
    }

    #endregion

    #region Write

    public bool WriteByteArray(IntPtr address, byte[] byteData)
    {
        return WriteProcessMemory(_handle, address, byteData, byteData.Length, IntPtr.Zero);
    }

    public bool WriteChar(IntPtr address, char value)
    {
        return WriteByteArray(address, BitConverter.GetBytes(value));
    }

    public bool WriteShort(IntPtr address, short value)
    {
        return WriteByteArray(address, BitConverter.GetBytes(value));
    }

    public bool WriteInt(IntPtr address, int value)
    {
        return WriteByteArray(address, BitConverter.GetBytes(value));
    }

    public bool WriteLong(IntPtr address, long value)
    {
        return WriteByteArray(address, BitConverter.GetBytes(value));
    }

    public bool WriteFloat(IntPtr address, float value)
    {
        return WriteByteArray(address, BitConverter.GetBytes(value));
    }

    public bool WriteDouble(IntPtr address, double value)
    {
        return WriteByteArray(address, BitConverter.GetBytes(value));
    }

    public bool WriteString(IntPtr address, string value)
    {
        return WriteByteArray(address, Encoding.Default.GetBytes(value));
    }

    #endregion

    #region Utils

    /// <summary>
    /// 根据 进程名 获取 PID
    /// </summary>    
    public static int GetPidByProcessName(string processName)
    {
        return GetProcessByProcessName(processName)?.Id ?? 0;
    }

    /// <summary>
    /// 通过 进程名(不加exe后缀) 获取 进程对象
    /// </summary>      
    public static Process? GetProcessByProcessName(string processName)
    {
        var processArr = Process.GetProcessesByName(processName);
        if (processArr.Length > 0)
        {
            return processArr[0];
        }

        return null;
    }

    /// <summary>
    /// 根据窗体标题查找窗口句柄（支持模糊匹配）
    /// </summary>    
    public static IntPtr FindWindow(string title)
    {
        var processArray = Process.GetProcesses();
        foreach (var item in processArray)
        {
            if (item.MainWindowTitle.IndexOf(title) != -1)
            {
                return item.MainWindowHandle;
            }
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// 获取进程中模块的基址 (例如: Game.dll / War3.exe)
    /// </summary>  
    public IntPtr GetModuleBaseAddress(string moduleName)
    {
        //var process = Process.GetProcessById(_gameInstance.MirPid);
        //IntPtr baseAddress = default;

        //for (int i = 0; i < process.Modules.Count; i++)
        //{
        //    var item = process.Modules[i];
        //    if (item.ModuleName == moduleName)
        //    {
        //        baseAddress = item.BaseAddress;
        //        break;
        //    }
        //}

        //return baseAddress;
        return 0x00;
    }


    /// <summary>
    /// 计算地址偏移
    /// </summary>   
    public IntPtr GetMemoryAddress(string moduleName, params nint[] offsetArray)
    {
        if ((offsetArray?.Length ?? 0) == 0)
        {
            throw new Exception("至少需要一个偏移");
        }

        IntPtr addr = IntPtr.Zero;

        // 模块的地址
        var addrVal = GetModuleBaseAddress(moduleName);

        // 计算剩下的多级偏移
        for (int i = 0; i < offsetArray!.Length; i++)
        {
            addr = IntPtr.Add(addrVal, (int)offsetArray[i]);
            addrVal = (IntPtr)ReadToInt(addr);
        }

        // 最终的地址 (只是一个地址, 需要手动去读里面的值)
        return addr;
    }

    public IntPtr GetMemoryAddress(params nint[] offsetArray)
    {
        return GetMemoryAddress("mir1", offsetArray);
    }

    /// <summary>
    /// 通过进程名 获取窗口句柄
    /// </summary>
    /// <param name="processName"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IntPtr GetWindowHwndByProcessName(string processName)
    {
        var hwnd = MemoryUtils.GetProcessByProcessName(processName)?.MainWindowHandle ?? IntPtr.Zero;

        if (hwnd == IntPtr.Zero)
        {
            throw new Exception($"没有找到[{processName}]进程..");
        }
        return hwnd;
    }

    #endregion

    #region 进制转换

    /// <summary>
    /// 16进制(0x41)转为10进制(65)
    /// </summary>   
    public static int ConvertFrom16To10(string value)
    {
        return Convert.ToInt32(value, 16);
    }

    /// <summary>
    /// 16进制(0x41)转为10进制(65)
    /// </summary>    
    public static int ConvertFrom16To10(IntPtr value)
    {
        return Convert.ToInt32(Convert.ToString(value));
    }

    /// <summary>
    /// 10进制(65)转为16进制(0x41)
    /// </summary>   
    public static string ConvertFrom10To16(IntPtr value)
    {
        //x4 0补齐4位
        //x8 0补齐8位
        return value.ToString("x");
    }

    /// <summary>
    /// 16/10进制转为2进制
    /// </summary>     
    public static string ConvertFrom16Or10To2(IntPtr value)
    {
        return Convert.ToString(value);
    }

    /// <summary>
    /// 2进制(1010)到10进制(2)
    /// </summary>    
    public static int ConvertFrom2To10(string value)
    {
        return Convert.ToInt32(value, 2);
    }
    #endregion

    /// <summary>
    /// 将字符串打包成nint数组用于传递给DLL
    /// </summary>
    /// <param name="strings">要打包的字符串</param>
    /// <returns>打包后的nint数组</returns>
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
