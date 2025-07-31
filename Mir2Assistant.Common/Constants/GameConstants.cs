namespace Mir2Assistant.Common.Constants;

public static class GameConstants
{
    public static class Skills
    {
        public const int HealSpellId = 2;
        public const int HealPeopleCD = 3000; // 治疗CD时间(ms)
    }

    public static class Items
    {
        public static readonly int buyCount = 16;
        public static readonly List<string> MegaPotions =
        new List<string>
        {
            "魔法药(小量)",
            "魔法药(中量)",
            "强效魔法药",
        };

        public static readonly List<string> binItems =
        new List<string>
        {
            "木剑",
            "布衣(男)",
            "布衣(女)",
            "匕首",
            "六角戒指",
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
    }
} 