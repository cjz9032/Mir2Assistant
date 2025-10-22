using System;

// U_DRESS = 0;
// U_WEAPON = 1;
// U_RIGHTHAND = 2;
// U_NECKLACE = 3;
// U_HELMET = 4;
// U_ARMRINGL = 5;
// U_ARMRINGR = 6;
// U_RINGL = 7;
// U_RINGR = 8;
// 2003/03/15 物品背包扩展
//    U_BUJUK        = 9;
//    U_BELT         = 10;
//    U_BOOTS        = 11;
//    U_CHARM        = 12;
//    U_TRANS        = 13;

    //  25:
    //      Result := U_BUJUK;//护身符
    //   54:
    //      Result := U_BELT;//腰带
    //   52:
    //      Result := U_BOOTS;//靴子
    //   53:
    //      Result := U_CHARM;//宝石
    //   65:
    //      Result := U_TRANS;//时装

//   case smode.StdMode of
//     5, 6: Result := U_WEAPON;
//     10, 11: Result := U_DRESS;
//     15: Result := U_HELMET;
//     19, 20, 21: Result := U_NECKLACE;
//     22, 23:
//       begin
//         Result := U_RINGL or U_RINGR
//       end;
//     24, 26:
//       begin
//         Result := U_ARMRINGR or U_ARMRINGL
//       end;
//     30: Result := U_RIGHTHAND;
//   end;
namespace Mir2Assistant.Common.Models
{
    public enum EquipPosition
    {
        /// <summary>
        /// 衣服
        /// </summary>
        Dress = 0,
        /// <summary>
        /// 武器
        /// </summary>
        Weapon = 1,
        /// <summary>
        /// 右手
        /// </summary>
        RightHand = 2,
        /// <summary>
        /// 项链
        /// </summary>
        Necklace = 3,
        /// <summary>
        /// 头盔
        /// </summary>
        Helmet = 4,
        /// <summary>
        /// 左手镯
        /// </summary>
        ArmRingLeft = 5,
        /// <summary>
        /// 右手镯
        /// </summary>
        ArmRingRight = 6,
        /// <summary>
        /// 左戒指
        /// </summary>
        RingLeft = 7,
        /// <summary>
        /// 右戒指
        /// </summary>
        RingRight = 8,
        /// <summary>
        /// 右戒指
        /// </summary>
        BUJUK = 9
    }
    public class ItemModel
    {

        public static readonly Dictionary<byte, byte[]> StdModeMap = new Dictionary<byte, byte[]> {
            {5, new byte[] {1}},
            {6, new byte[] {1}},
            {10, new byte[] {0}},
            {11, new byte[] {0}},
            {15, new byte[] {4}},
            {19, new byte[] {3}},
            {20, new byte[] {3}},
            {21, new byte[] {3}},
            {22, new byte[] {7, 8}},
            {23, new byte[] {7, 8}},
            {24, new byte[] {5, 6}},
            {25, new byte[] {5, 9}}, //  todo 25和毒品
            {26, new byte[] {5, 6}},
            {30, new byte[] {2}}
        };

        public int Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsEmpty { get; set; }
        public bool IsGodly { get; set; }
        public int GodPts { get; set; }
        public byte stdMode { get; set; }
        public byte reqType { get; set; }
        public byte reqPoints { get; set; }
        public int Duration { get; set; }
        public int addr { get; set; }
        public int MaxDuration { get; set; }
        public int MinDef { get; set; }
        public int Luck { get; set; }
        public int MacEvasion { get; set; }
        public int MaxDef { get; set; }
        public int MinMageDef { get; set; }
        public int MaxMageDef { get; set; }
        public short Looks { get; set; }

        // 原始10属性 Ac Ac2	Mac	Mac2	Dc	Dc2	Mc	Mc2	Sc	Sc2
        public byte[] OriginCriticals = new byte[10];
        // public int OAc2 { get; set; }
        // public int OMac { get; set; }
        // public int OMac2 { get; set; }
        // public int ODc { get; set; }
        // public int ODc2 { get; set; }
        // public int OMc { get; set; }
        // public int OMc2 { get; set; }
        // public int OSc { get; set; }
        // public int OSc2 { get; set; }

        public string Display => IsEmpty ? "" : $"{Index}: [{Id:x2}] {(IsGodly ? $"（Jesus-{GodPts}）" : "")} {(IsLowDurability ? "LOW!!! " : "")}" + PadStringWithFullWidthSpaces(Name, 4) + $" {Duration}/{MaxDuration} sm{stdMode} {addr:x2}";
        public bool IsLowDurability => Duration > 0 ? (((double)Duration / MaxDuration) < 0.25) : false;
        public byte[] stdModeToUseItemIndex
        {
            get
            {
                if (StdModeMap.TryGetValue(stdMode, out byte[] result))
                {
                    return result;
                }
                return new byte[] { 255 };
            }
        }

        private string PadStringWithFullWidthSpaces(string input, int length)
        {
            int charCount = 0;
            foreach (char c in input)
            {
                charCount += char.GetUnicodeCategory(c) <= System.Globalization.UnicodeCategory.OtherLetter ? 2 : 1;
            }
            int fullWidthSpacesNeeded = length * 2 - charCount;
            if (fullWidthSpacesNeeded > 0)
            {
                return input + new string('　', fullWidthSpacesNeeded / 2);
            }
            return input;
        }

        public ItemModel(int idx = 0)
        {
            IsEmpty = true;
            Index = idx;
        }
    }
    public class DropItemModel
    {
        public int Id { get; set; }
        public int UpdateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsGodly { get; set; }
        public int GodPts { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsEmpty { get; set; }
        public short Looks { get; set; }

        // public string Display => IsEmpty ? "" : $"{Index}: [{Id:x2}] {(IsGodly ? "（Jesus）" : "")} {(IsLowDurability ? "LOW!!! " : "")}"  + PadStringWithFullWidthSpaces(Name, 4) + $" {Duration}/{MaxDuration} sm{stdMode}";
    }
}