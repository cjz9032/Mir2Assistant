using System.Collections.Frozen;
using Mir2Assistant.Common.Functions;
using Mir2Assistant.Common.Models;

namespace Mir2Assistant.Common.Constants;

public static class GameConstants
{
    public const int NoobLevel = 7;
    public const int MidLevel = 11;

    public static class Skills
    {
        public const int fireBall = 1;
        public const int MagePush = 8;
        public const int TemptationSpellId = 20;
        public const int LightingSpellId = 11;
        public const int temptSpellId = 20;
        public const int baolie = 23;
        public const int mageDefup = 14;
        public const int flashMove = 21;
        public const int defUp = 15;
        public const int HealSpellId = 2;
        public const int RecallBoneSpellId = 17;
        public const int HealPeopleCD = 2500; // 治疗CD时间(ms)
    }

    public static readonly Dictionary<int, int> MagicSpellMap = new()
    {
        { 1, 4 }, { 2, 7 }, { 3, 0 }, { 4, 0 }, { 5, 3 }, { 6, 4 }, { 7, 0 }, { 8, 8 }, { 9, 10 },
        { 10, 30 }, { 11, 12 }, { 12, 0 }, { 13, 5 }, { 14, 15 }, { 15, 15 }, { 16, 10 }, { 17, 16 },
        { 18, 5 }, { 19, 10 }, { 20, 3 }, { 21, 10 }, { 22, 20 }, { 23, 15 }, { 24, 35 }, { 25, 0 },
        { 26, 0 }, { 27, 15 }, { 28, 16 }, { 29, 12 }, { 30, 16 }, { 31, 20 }, { 32, 50 }, { 33, 12 }
    };

    // 根据等级返回刀（战士）身体防御区间（最小值, 最大值）
    public static (int min, int max) GetBladeBodyDefByLevel(int level)
    {
        if (level <= 0) return (0, 0);
        if (level <= 6) return (0, 0);   // 1-6  -> 0-0
        if (level <= 13) return (0, 1);  // 7-13 -> 0-1
        if (level <= 20) return (0, 2);  // 14-20 -> 0-2
        if (level <= 27) return (0, 3);  // 21-27 -> 0-3
        if (level <= 34) return (0, 4);  // 28-34 -> 0-4
        if (level <= 41) return (0, 5);  // 35-41 -> 0-5
        if (level <= 48) return (0, 6);  // 42-48 -> 0-6
        return (0, 6);                   // 48+
    }

    // 根据等级返回道士（Toast）身体魔御区间（最小值, 最大值）
    public static (int min, int max) GetToastBodyMageDefByLevel(int level)
    {
        if (level <= 0) return (0, 1);
        if (level <= 5) return (0, 1);    // 1-5  -> 0-1
        if (level <= 11) return (0, 2);   // 6-11 -> 0-2
        if (level <= 17) return (1, 3);   // 12-17 -> 1-3
        if (level <= 23) return (1, 4);   // 18-23 -> 1-4
        if (level <= 29) return (2, 5);   // 24-29 -> 2-5
        if (level <= 35) return (2, 6);   // 30-35 -> 2-6
        if (level <= 41) return (3, 7);   // 36-41 -> 3-7
        if (level <= 47) return (3, 8);   // 42-47 -> 3-8
        return (3, 8);                    // 47+
    }

    public static readonly string[] allowM10 = new string[] { "鹿", "羊", "鸡" };
    public static readonly string[] allowTemp24 = new string[] { "洞蛆", "威思而小虫", "蜈蚣" };
    public static readonly string[] allowTemp26 = new string[] { "黑色恶蛆", "虎卫" };
    public static readonly string[] allowTemp29 = new string[] { "虎卫", "钳虫" };
    public static readonly string[] allow15 = new string[] { "稻草人", "多钩猫", "钉耙猫", "蛤蟆" };
    public static readonly string[] allow22 = new string[] { "半兽人", "森林雪人", "毒蜘蛛", "威思而小虫" };
    public static readonly string[] allowMonsters = new string[]  {   "食人花", "虎蛇" , "红蛇","半兽战士", "半兽勇士", "蝎子"
            , "洞蛆", "蝙蝠", "骷髅", "骷髅战将", "掷斧骷髅", "骷髅战士", "骷髅精灵", "僵尸","山洞蝙蝠", "尸王",
        "红野猪", "黑野猪", "楔蛾", "蝎蛇", "角蝇", 
        "蜈蚣", "黑色恶蛆", "跳跳蜂", "钳虫", "巨型蠕虫", "邪恶钳虫" };

 public static string[] GetAllowTemp(int level)
        {
            if (level <= 25)
            {
                return allowTemp24;
            }
            else if (level <= 26)
            {
                return allowTemp26;
            }
            else if (level <= 29)
            {
                return allowTemp29;
            }
            return allowTemp29;
        }
    public static string[] GetAllowMonsters(int level, RoleType role)
    {
        var offset = role == RoleType.blade ? -2 : 0;

        if (level < (NoobLevel + offset))
        {
            var temp = allowM10.Concat(allow15).ToList();
            return temp.ToArray();
        }
        else if (level < MidLevel + offset)
        {
            var temp = allowM10.Concat(allow15).Concat(allow22).ToList();
            return temp.ToArray();

        }
        else if (level < 16 + offset)
        {
            return allowMonsters.Concat(allow15).Concat(allow22).ToArray();
        }
        else if (level < 50 + offset)
        {
            return allowMonsters.Concat(allow15).Concat(allow22).ToArray();
        }
        return allowMonsters;
    }

    public static class Items
    {
        public static readonly string[] weaponList = new string[] {
            "鹤嘴锄"   ,
            "裁决之杖"  ,
            "骨玉权杖"  ,
            "海魂"    ,
            "炼狱"    ,
            "魔杖"    ,
            "偃月"    ,
            "木剑"    ,
            "铁剑"    ,
            "青铜剑"   ,
            "凝霜"    ,
            "短剑"    ,
            "青铜斧"   ,
            "匕首"    ,
            "井中月"   ,
            "银蛇"    ,
            "修罗"    ,
            "凌风"    ,
            "破魂"    ,
            "斩马刀"   ,
            "乌木剑"   ,
            "八荒"    ,
            "半月"    ,
            "降魔"    ,
            "无极棍"   ,
            "血饮"    ,
            "祈祷之刃"  ,
            "命运之刃"  ,
            "龙纹剑"   ,
    };
        public static readonly int mageBuyCount = 30; // todo by level
        public static readonly int superPickCount = 3;
        public static readonly int healBuyCount = 4;
        // public static readonly int keepWeaponCount = 2;
        // public static readonly int keepClothCount = 2;
        public static int getKeepWeaponCount(int level, RoleType role)
        {
            return level >= 22 ? 1 : 2;
        }
        public static int getKeepClothCount(int level, RoleType role)
        {
            return level >= 22 ? 1 : 2;
        }
        public static readonly List<string> MagePotions =
        new List<string>
        {
            "魔法药(小量)",
            "魔法药(中量)",
            "强效魔法药",
        };
        public static readonly List<string> HealPotions =
        new List<string>
        {
        "金创药(小量)", "金创药(中量)", "强效金创药",

        };


        public static readonly FrozenSet<string> JPSetFrozen =
            new[] { "祝福油", "山洞凭证", "半月弯刀", "困魔咒", "烈火剑法", "召唤神兽", "冰咆哮", "地狱雷光","鹤嘴锄",
                "魔法盾", "火墙", "群体治疗术", "刺杀剑术", "野蛮冲撞", "圣言术", "极速神水","体力强效药水", "战神油" }.ToFrozenSet(StringComparer.Ordinal);

        public static readonly List<string> SuperPotions =
      new List<string>
      {
    "太阳水",
            "强效太阳水",
            "万年雪霜"
      };

        private static readonly List<string> binItems =
     new List<string>
     {
            "火球术",
            "大火球",
            "治愈术",
            "基本剑术",
            "精神力战法",
            "地狱火",
            "施毒术",
            "攻杀剑术",
            "抗拒火环",
            "雷电术",
            "隐身术",
            "集体隐身术",
            "诱惑之光",
            "瞬息移动",
            "神圣战甲术",
            "疾光电影",
            "召唤骷髅",
            "灵魂火符",
            "幽灵盾",
            // "肉",
            // "护身符",
            "随机传送卷",
            // "鸡肉",
            "食人树叶",
            "木剑",
            "布衣(男)",
            "布衣(女)",
          //  "短剑",
          //  "青铜斧",
            "八荒",
            "玻璃戒指",
            "黑檀项链",
            "黑色水晶项链",
            "黄色水晶项链",
            "白色虎齿项链",
            "六角戒指",
            "琥珀项链",
            "小手镯",
            "银手镯",
            // lj书
     };
        // 新手装

        private static readonly List<string> binItemsNoob =
           new List<string>
           {
            "古铜戒指",
            // "牛角戒指",
            // "金项链",
            "传统项链",
            "铁手镯",
            "皮制手套",
            // "钢手镯",
            "匕首",
            "青铜剑",
            // "铁剑",
            "青铜头盔",
  
           };

        private static readonly List<string> binItemsMid =
  new List<string>
  {
            "牛角戒指",
            "金项链",
            "钢手镯",
            "铁剑",
            "短剑",
            "青铜斧",
            "魔鬼项链",
            "蓝色水晶戒指",
  };
        public static List<string> GetBinItems(MirGameInstanceModel gameInstance, int level, RoleType role)
        {
            if (level < 14)
            {
                return binItems;
            }
            var temp = binItems.Concat(binItemsNoob).ToList();
            if (role == RoleType.blade)
            {
                temp.Remove("八荒");
            }
            if (level < 20)
            {
                if (role == RoleType.mage)
                {
                    temp.Remove("六角戒指");
                    temp.Remove("琥珀项链");
                }
                return temp.ToList();
            }
            if (role != RoleType.taoist)
            {
                temp.Add("护身符");
                var canDef = GoRunFunction.CapbilityOfDefUp(gameInstance);
                var canMageDef = GoRunFunction.CapbilityOfMageDefUp(gameInstance);
                if (!canDef)
                {
                    temp.Remove("神圣战甲术");
                }
                if (!canMageDef)
                {
                    temp.Remove("幽灵盾");
                }
            }
            if (role != RoleType.mage)
            {
                temp.Add("生铁戒指");
            }

            return temp.Concat(binItemsMid).ToList();
        }
    }

    /// <summary>
    /// 怪物等级映射表 - 用于快速检索怪物等级
    /// </summary>
    public static readonly Dictionary<string, int> TempMonsterLevels = new Dictionary<string, int>
    {
        ["鸡"] = 5,
        ["鹿"] = 12,
        ["蛤蟆"] = 12,
        ["多钩猫"] = 13,
        ["钉耙猫"] = 13,
        ["羊"] = 13,
        ["半兽人"] = 15,
        ["森林雪人"] = 16,
        ["盔甲虫"] = 16,
        ["猎鹰"] = 16,
        ["威思而小虫"] = 16,
        ["多角虫"] = 16,
        ["毒蜘蛛"] = 17,
        ["红蛇"] = 17,
        ["沙虫"] = 17,
        ["蝎子"] = 18,
        ["虎蛇"] = 18,
        ["食人花"] = 20,
        ["山洞蝙蝠"] = 20,
        ["半兽战士"] = 21,
        ["洞蛆"] = 21,
        ["暗黑战士"] = 26,
        ["蜈蚣"] = 26,
        ["半兽勇士"] = 28,
        ["巨型多角虫"] = 28,
        ["黑色恶蛆"] = 28,
        ["钳虫"] = 31,
        ["楔蛾"] = 32
    };
}