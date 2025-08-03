namespace Mir2Assistant.Common.Constants;

public static class GameConstants
{
    public static class Skills
    {
        public const int HealSpellId = 2;
        public const int HealPeopleCD = 2500; // 治疗CD时间(ms)
    }
    
    public static readonly string[] allowM10 = new string[] { "鸡", "鹿", "羊" };
    public static readonly string[] allow15 = new string[] { "稻草人", "多钩猫", "钉耙猫", "蛤蟆" };
    public static readonly string[] allow22 = new string[] { "半兽人","森林雪人", "食人花", "毒蜘蛛" };
    public static readonly string[] allowMonsters = new string[]  {   "半兽战士", "半兽勇士", "蝎子"
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

        public static readonly List<string> binItems =
     new List<string>
     {
            "肉",
            "鸡肉",
            "食人树叶",
            "木剑",
            "布衣(男)",
            "布衣(女)",
            "匕首",
            "青铜剑",
            "铁剑",
            "短剑",
            "青铜斧",
            "古铜戒指",
            "六角戒指",
            "牛角戒指",
            "玻璃戒指",
            "金项链",
            "传统项链",
            "黑檀项链",
            "黑色水晶项链",
            "黄色水晶项链",
            "铁手镯",
            "小手镯",
            "皮质手套",
            "银手镯",
            "传统项链",
            // lj书
            "治愈术",
            "基本剑术",
            "火球术",
     };
    }
} 