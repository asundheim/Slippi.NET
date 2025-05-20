namespace Slippi.NET.Stats.Types;

public record class ActionCounts
{
    public required int PlayerIndex { get; set; }
    public required int WavedashCount { get; set; }
    public required int WavelandCount { get; set; }
    public required int AirDodgeCount { get; set; }
    public required int DashDanceCount { get; set; }
    public required int SpotDodgeCount { get; set; }
    public required int LedgegrabCount { get; set; }
    public required int RollCount { get; set; }
    public required LCancelCounts LCancelCount { get; set; }
    public required AttackCounts AttackCount { get; set; }
    public required GrabCounts GrabCount { get; set; }
    public required ThrowCounts ThrowCount { get; set; }
    public required GroundTechCounts GroundTechCount { get; set; }
    public required WallTechCounts WallTechCount { get; set; }
}

public record class LCancelCounts
{
    public required int Success { get; set; }
    public required int Fail { get; set; }
}

public record class AttackCounts
{
    public required int Jab1 { get; set; }
    public required int Jab2 { get; set; }
    public required int Jab3 { get; set; }
    public required int Jabm { get; set; }
    public required int Dash { get; set; }
    public required int Ftilt { get; set; }
    public required int Utilt { get; set; }
    public required int Dtilt { get; set; }
    public required int Fsmash { get; set; }
    public required int Usmash { get; set; }
    public required int Dsmash { get; set; }
    public required int Nair { get; set; }
    public required int Fair { get; set; }
    public required int Bair { get; set; }
    public required int Uair { get; set; }
    public required int Dair { get; set; }
}

public record class GrabCounts
{
    public required int Success { get; set; }
    public required int Fail { get; set; }
}

public record class ThrowCounts
{
    public required int Up { get; set; }
    public required int Forward { get; set; }
    public required int Back { get; set; }
    public required int Down { get; set; }
}

public record class GroundTechCounts
{
    public required int Away { get; set; }
    public required int In { get; set; }
    public required int Neutral { get; set; }
    public required int Fail { get; set; }
}

public record class WallTechCounts
{
    public required int Success { get; set; }
    public required int Fail { get; set; }
}