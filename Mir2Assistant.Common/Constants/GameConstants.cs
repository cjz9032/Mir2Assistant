namespace Mir2Assistant.Common.Constants;

public static class GameConstants
{
    public static class Skills
    {
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


    public static readonly string[] allowM10 = new string[] { "鸡", "鹿", "羊" };
    public static readonly string[] allow15 = new string[] { "稻草人", "多钩猫", "钉耙猫", "蛤蟆" };
    public static readonly string[] allow22 = new string[] { "半兽人","森林雪人", "食人花", "毒蜘蛛" };
    public static readonly string[] allowMonsters = new string[]  {   "虎蛇" , "红蛇","半兽战士", "半兽勇士", "蝎子"
            , "洞蛆", "蝙蝠", "骷髅", "骷髅战将", "掷斧骷髅", "骷髅战士", "骷髅精灵", "僵尸","山洞蝙蝠"};

    public static string[] GetAllowMonsters(int level)
    {
        if (level < 10)
        {
            return allowMonsters.Concat(allowM10).Concat(allow15).Concat(allow22).ToArray();
        }
        else if (level < 15)
        {
            return allowMonsters.Concat(allow15).Concat(allow22).ToArray();
        }
        else if (level < 22)
        {
            return allowMonsters.Concat(allow22).ToArray();
        }
        return allowMonsters;
    }

    public static class Items
    {
        public static readonly int megaBuyCount = 10;
        public static readonly int healBuyCount = 6;
        public static readonly List<string> MegaPotions =
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


        public static readonly List<string> SuperPotions =
      new List<string>
      {
    "太阳水",
            "强效太阳水"
      };

        private static readonly List<string> binItems =
     new List<string>
     {
            "火球术",
            "治愈术",
            "基本剑术",
            "精神力战法",
            "肉",
            "护身符",
            "随机传送卷",
            "鸡肉",
            "食人树叶",
            "木剑",
            "布衣(男)",
            "布衣(女)",
            "短剑",
            "青铜斧",
            "六角戒指",
            "玻璃戒指",
            "黑檀项链",
            "黑色水晶项链",
            "黄色水晶项链",
            "小手镯",
            "银手镯",
            // lj书
            "治愈术",
            "基本剑术",
            "火球术",
     };
        // 新手装

        private static readonly List<string> binItemsNoob =
           new List<string>
           {
            "古铜戒指",
            "牛角戒指",
            "金项链",
            "传统项链",
            "铁手镯",
            "皮质手套",
            "钢手镯",
            "匕首",
            "青铜剑",
            "铁剑",
            "青铜头盔",
           };
        public static List<string> GetBinItems(int level)
        {
            if (level < 10)
            {
                return binItems;
            }
            return binItems.Concat(binItemsNoob).ToList();
        }
    }
    
} 