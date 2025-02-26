using Mir2Assistant.Common.Utils;
using System.Collections.Concurrent;

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


    public byte MonstersUpdateId = 0;
    /// <summary>
    /// 怪物
    /// </summary>
    public ConcurrentDictionary<int, MonsterModel> Monsters { get; set; } = new ConcurrentDictionary<int, MonsterModel>();

    public bool IsReadingMonsters = false;

    /// <summary>
    /// 技能
    /// </summary>
    public ConcurrentBag<SkillModel> Skills { get; set; } = new ConcurrentBag<SkillModel>();

    /// <summary>
    /// 二级对话列表
    /// </summary>
    public List<string> TalkCmds { get; set; } = new List<string>();

    public nint? SysMsgAddr;
    public event Action<string?>? NewSysMsg;
    private string? _SysMsg;
    public string? SysMsg
    {
        get
        {
            return _SysMsg;
        }
        set
        {
            if (_SysMsg != value)
            {
                NewSysMsg?.Invoke(value);
                _SysMsg = value;
            }
        }
    }

}