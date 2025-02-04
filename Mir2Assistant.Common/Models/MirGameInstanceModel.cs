using Mir2Assistant.Common.Utils;

namespace Mir2Assistant.Common.Models;

public class MirGameInstanceModel
{
    public int MirPid { get; set; } //游戏pid
    public uint MirThreadId { get; set; }//游戏主线程ID
    public IntPtr MirHwnd { get; set; }//游戏主窗体句柄
    public nint MirBaseAddress { get; set; }
    public string? mirVer { get; set; }
    public nint? HookHandle { get; set; }
    public IntPtr? LibIpdl { get; set; }
    public Form? AssistantForm { get; set; }
    public Dictionary<string, nint> MirConfig = new Dictionary<string, nint>();

    public CharacterStatusModel? CharacterStatus { get; set; } = new CharacterStatusModel();
    public MemoryUtils? MemoryUtils { get; set; }
}
