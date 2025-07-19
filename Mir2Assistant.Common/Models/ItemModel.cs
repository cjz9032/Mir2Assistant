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
    public class ItemModel
    {
        public static readonly Dictionary<int, int> StdModeMap = new Dictionary<int, int> {
            {5, 1},
            {6, 1},
            {10, 0},
            {11, 0},
            {15, 4},
            {19, 3},
            {20, 3},
            {21, 3},
            // 7,8
            {22, 7},
            {23, 7},
            // 5,6
            {24, 5},
            {26, 5},
            {30, 2}
        };

        public int Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsEmpty { get; set; }
        public bool IsGodly { get; set; }
        public int stdMode { get; set; }
        public int reqType { get; set; }
        public int reqPoints { get; set; }
        public int Duration { get; set; }
        public int MaxDuration { get; set; }
        public string Display => IsEmpty ? "" : $"{Index}: [{Id:x2}] {(IsGodly ? "（Jesus）" : "")} {(IsLowDurability ? "LOW!!! " : "")}"  + PadStringWithFullWidthSpaces(Name, 4) + $" {Duration}/{MaxDuration} sm{stdMode}";
        public bool IsLowDurability => Duration > 0 ? (((double)Duration / MaxDuration) < 0.25) : false;
        public int stdModeToUseItemIndex
        {
            get
            {
                if (StdModeMap.TryGetValue(stdMode, out int result))
                {
                    return result;
                }
                return -1;
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
}