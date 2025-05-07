namespace Slippi.NET.Types;

public record class GameInfoType
{
    public GameInfoType(
        int? gameBitfield1, 
        int? gameBitfield2, 
        int? gameBitfield3, 
        int? gameBitfield4, 
        bool? bombRainEnabled, 
        int? selfDestructScoreValue, 
        int? itemSpawnBitfield1, 
        int? itemSpawnBitfield2, 
        int? itemSpawnBitfield3, 
        int? itemSpawnBitfield4, 
        int? itemSpawnBitfield5, 
        float? damageRatio)
    {
        GameBitfield1 = gameBitfield1;
        GameBitfield2 = gameBitfield2;
        GameBitfield3 = gameBitfield3;
        GameBitfield4 = gameBitfield4;
        BombRainEnabled = bombRainEnabled;
        SelfDestructScoreValue = selfDestructScoreValue;
        ItemSpawnBitfield1 = itemSpawnBitfield1;
        ItemSpawnBitfield2 = itemSpawnBitfield2;
        ItemSpawnBitfield3 = itemSpawnBitfield3;
        ItemSpawnBitfield4 = itemSpawnBitfield4;
        ItemSpawnBitfield5 = itemSpawnBitfield5;
        DamageRatio = damageRatio;
    }

    public int? GameBitfield1 { get; set; }
    public int? GameBitfield2 { get; set; }
    public int? GameBitfield3 { get; set; }
    public int? GameBitfield4 { get; set; }
    public bool? BombRainEnabled { get; set; }
    public int? SelfDestructScoreValue { get; set; }
    public int? ItemSpawnBitfield1 { get; set; }
    public int? ItemSpawnBitfield2 { get; set; }
    public int? ItemSpawnBitfield3 { get; set; }
    public int? ItemSpawnBitfield4 { get; set; }
    public int? ItemSpawnBitfield5 { get; set; }
    public float? DamageRatio { get; set; }
}