namespace Mir2Assistant.Common.Models;

public class CharacterStatusModel
{
    public string Name { get; set; } = string.Empty;
    public string MapName { get; set; } = string.Empty;
    public string MapId { get; set; } = string.Empty;
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int CurrentHP { get; set; } = 0;
    public int MaxHP { get; set; } = 0;
    public int CurrentMP { get; set; } = 0;
    public int MaxMP { get; set; } = 0;
    public byte Level { get; set; } = 0;
    public byte groupMemCount { get; set; } = 0;
    public bool allowGroup { get; set; } = false;
    public int coin { get; set; } = 0;

    public int MinDef { get; set; } = 0;
    public int MaxDef { get; set; } = 0;
    public bool isDead { get; set; } = true;

    public int MinMageDef { get; set; } = 0;
    public int MaxMageDef { get; set; } = 0;
    public int Exp { get; set; } = 0;
    public int MaxExp { get; set; } = 0;

    public bool isEnhanceDead => isDead || (CurrentHP <= 0 && X <= 0);

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
