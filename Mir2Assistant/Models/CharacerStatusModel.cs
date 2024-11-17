namespace Mir2Assistant.Models
{
    public class CharacerStatusModel
    {
        public string? Name { get; set; }
        public string? MapName { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public int? CurrentHP { get; set; }
        public int? MaxHP { get; set; }
        public int? CurrentMP { get; set; }
        public int? MaxMP { get; set; }
        /// <summary>
        /// 转生等级
        /// </summary>
        public int? GradeZS { get; set; }
    }
}
