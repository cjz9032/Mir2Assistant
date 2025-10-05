using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Utils;
using System.Collections.Concurrent;

namespace Mir2Assistant.Common.Models;
using Serilog; // 新增Serilog引用
public class MirGameInstanceModel
{
    public bool IsBotRunning { get; set; }
    public int MirPid { get; set; } //游戏pid
    public int spellLastTime { get; set; } //技能 cd
    public int bladeCiciLastTime { get; set; } //武士cici cd
    public int eatItemLastTime { get; set; } //吃物品 cd
    public LimitedDictionary<int, int> healCD { get; set; } = new LimitedDictionary<int, int>();
    public uint MirThreadId { get; set; }//游戏主线程ID
    public IntPtr MirHwnd { get; set; }//游戏主窗体句柄
    public nint MirBaseAddress { get; set; }
    public string? mirVer { get; set; }
    public nint? HookHandle { get; set; }
    public IntPtr? LibIpdl { get; set; }
    public Form AssistantForm { get; set; }

    public CharacterStatusModel CharacterStatus { get; set; } = new CharacterStatusModel();
    public MemoryUtils memoryUtils { get; set; }

    public GameAccountModel AccountInfo { get; set; }

    public byte MonstersUpdateId = 0;
    public byte DropsItemsUpdateId = 0;
    /// <summary>
    /// 怪物
    /// </summary>
    public ConcurrentDictionary<int, MonsterModel> Monsters { get; set; } = new ConcurrentDictionary<int, MonsterModel>();
    // drops
    public ConcurrentDictionary<int, DropItemModel> DropsItems { get; set; } = new ConcurrentDictionary<int, DropItemModel>();
    // 捡取过的ItemID名单, 高性能检索
    public LimitedHashSet<int> pickupItemIds { get; set; } = new LimitedHashSet<int>(10);

    // 攻击过的怪物ID名单, 高性能检索
    public LimitedHashSet<int> attackedMonsterIds { get; set; } = new LimitedHashSet<int>(100);
    // 法师专打名单过的怪物ID名单
    public LimitedDictionary<int, int> mageDrawAttentionMonsterCD { get; set; } = new LimitedDictionary<int, int>();
    public int mageDrawAttentionGlobalCD { get; set; } //技能 cd


    public bool IsReadingMonsters = false;
    public bool IsReadingItems = false;
    public bool isRestarting = false;
    public bool isHomePreparing = false;
    public bool isPickingWay = false;
    public int sameHPtimes = 0;
    public int lastHP = 0;
    public List<string> chats = new List<string>();

    /// <summary>
    /// 技能
    /// </summary>
    public ConcurrentBag<SkillModel> Skills { get; set; } = new ConcurrentBag<SkillModel>();

    public List<ItemModel> QuickItems { get; set; } = new List<ItemModel>();

    public List<ItemModel> Items { get; set; } = new List<ItemModel>();

    public MirGameInstanceModel()
    {
        for (int i = 0; i < 6; i++)
        {
            QuickItems.Add(new ItemModel(i));
        }

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
    public Dictionary<string, (int, int, byte[])> MapBasicInfo { get; set; } = new Dictionary<string, (int, int, byte[])>();

    // 需要clear方法, 因为实例还在 但是数据要清空
    public void Clear()
    {
        Monsters.Clear();
        // 不销毁
        foreach (var item in DropsItems)
        {
            item.Value.IsEmpty = true;
            item.Value.Name = "";
        }
        foreach (var item in QuickItems)
        {
            item.IsEmpty = true;
            item.Name = "";
        }
        foreach (var item in Items)
        {
            item.IsEmpty = true;
            item.Name = "";
        }

        Skills.Clear();
        TalkCmds.Clear();
        chats = new List<string>();
        // MapBasicInfo.Clear(); 保留把, 固定的信息
        // CharacterStatus = new CharacterStatusModel();
        CharacterStatus.CurrentHP = 0;
        CharacterStatus.X = 0;
        CharacterStatus.Y = 0;
        CharacterStatus.MapId = "";
        CharacterStatus.MapName = "";

        // 别销毁
        //AccountInfo = new GameAccountModel();
        MirPid = 0;
        spellLastTime = 0;
        bladeCiciLastTime = 0;
        eatItemLastTime = 0;
        MirThreadId = 0;
        sameHPtimes = 0;
        lastHP = 0;
        memoryUtils?.Dispose();
        isHomePreparing = false;
        isPickingWay = false;
    }

    // 直接用属性pid来判断, 快但是可能不准, 后面需要确认是不是及时更新的, 及时就没问题
    public bool IsAttached => MirPid != 0 && memoryUtils != null;

    public bool isRefreshing = false;

    public void RefreshAll()
    {
        if (isRefreshing)
        {
            return;
        }
        isRefreshing = true;
        try
        {
            CharacterStatusFunction.GetInfo(this, true);
            CharacterStatusFunction.GetUsedItemInfo(this, true);
            CharacterStatusFunction.ReadChats(this, true);
            MonsterFunction.ReadMonster(this, true);
            ItemFunction.ReadBag(this, true);
            ItemFunction.ReadDrops(this, true);
            SkillFunction.ReadSkills(this);
        }
        catch (Exception ex)
        {
            Log.Error("刷新失败: {Error}", ex.Message);
        }

        isRefreshing = false;
    }
}