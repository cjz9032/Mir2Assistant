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

    public GameAccountModel? AccountInfo { get; set; }
    
    public byte MonstersUpdateId = 0;
    public byte ItemsUpdateId = 0;
    /// <summary>
    /// 怪物
    /// </summary>
    public ConcurrentDictionary<int, MonsterModel> Monsters { get; set; } = new ConcurrentDictionary<int, MonsterModel>();

    public bool IsReadingMonsters = false;
    public bool IsReadingItems = false;

    /// <summary>
    /// 技能
    /// </summary>
    public ConcurrentBag<SkillModel> Skills { get; set; } = new ConcurrentBag<SkillModel>();
    public List<ItemModel> Items { get; set; } = new List<ItemModel>();

    public MirGameInstanceModel()
    {
        for (int i = 0; i < 40; i++)
        {
            Items.Add(new ItemModel(i));
        }
    }

    /// <summary>
    /// 二级对话列表
    /// </summary>
    public List<string> TalkCmds { get; set; } = new List<string>();

    public event Action<nint, string?>? NewSysMsg;
    public void InvokeSysMsg(nint flag, string? msg)
    {
        Task.Run(() =>
        {
            NewSysMsg?.Invoke(flag, msg);
        });
      
    }

}