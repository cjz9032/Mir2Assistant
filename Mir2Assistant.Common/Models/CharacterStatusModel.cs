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
    public int state = 0;
    public bool isHidden => state == 0x00800000;
//    POISON_DECHEALTH     = 0;   //$80000000 绿毒
//    POISON_DAMAGEARMOR   = 1;   //$40000000 红毒
//    POISON_ICE           = 2;   //$20000000
//    POISON_STUN          = 3;   //$10000000
//    POISON_SLOW          = 4;   //$08000000
//    POISON_STONE         = 5;   //$04000000
//    POISON_DONTMOVE      = 6;   //$02000000

//    STATE_BLUECHAR       = 2;
//    STATE_FASTMOVE       = 7;   //$01000000
//    STATE_TRANSPARENT    = 8;   //$00800000
//    STATE_DEFENCEUP      = 9;   //$00400000
//    STATE_MAGDEFENCEUP   = 10;  //$00200000
//    STATE_BUBBLEDEFENCEUP = 11; //$00100000
    // 装备9格
    public List<ItemModel> useItems { get; set; } = new List<ItemModel>();

    public CharacterStatusModel()
    {
        // todo BUJUK 9+
        for (int i = 0; i < 10; i++)
        {
            useItems.Add(new ItemModel(i));
        }
    }
}
