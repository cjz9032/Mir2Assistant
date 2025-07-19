using System;

namespace Mir2Assistant.Common.Models
{
    public class ItemModel
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsEmpty { get; set; }
        public bool IsGodly { get; set; }
        public int Duration { get; set; }
        public int MaxDuration { get; set; }
        public string Display => IsEmpty ? "" : $"{Index}: [{Id:x2}] {(IsGodly ? "（Jesus）" : "")} {(IsLowDurability ? "LOW!!! " : "")}"  + PadStringWithFullWidthSpaces(Name, 4) + $" {Duration}/{MaxDuration}";
        public bool IsLowDurability => Duration > 0 ? (Duration / MaxDuration) < 0.25 : false;

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