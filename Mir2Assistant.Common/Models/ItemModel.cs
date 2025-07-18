using System;

namespace Mir2Assistant.Common.Models
{
    public class ItemModel
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsEmpty { get; set; }
        public string Display => IsEmpty ? "" : $"{Index}: [{Id:x2}] {Name}";
    }
}