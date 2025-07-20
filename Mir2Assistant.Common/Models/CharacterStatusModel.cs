namespace Mir2Assistant.Common.Models;

public class CharacterStatusModel
{
    public string Name { get; set; }
    public string MapName { get; set; }
    public string MapId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    public int CurrentMP { get; set; }
    public int MaxMP { get; set; }
    public byte Level { get; set; }
    public byte groupMemCount { get; set; }
    public bool allowGroup { get; set; }

    // 装备9格
    public List<ItemModel> useItems { get; set; } = new List<ItemModel>();

    public CharacterStatusModel()
    {
        for (int i = 0; i < 9; i++)
        {
            useItems.Add(new ItemModel(i));
        }
    }
}
