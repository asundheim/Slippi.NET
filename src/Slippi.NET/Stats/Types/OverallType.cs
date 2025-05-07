namespace Slippi.NET.Stats.Types;

public record class OverallType
{
    public required int PlayerIndex { get; init; }
    public required InputCountsType InputCounts { get; init; }
    public required int ConversionCount { get; init; }
    public required float TotalDamage { get; init; }
    public required int KillCount { get; init; }
    public required RatioType SuccessfulConversions { get; init; }
    public required RatioType InputsPerMinute { get; init; }
    public required RatioType DigitalInputsPerMinute { get; init; }
    public required RatioType OpeningsPerKill { get; init; }
    public required RatioType DamagePerOpening { get; init; }
    public required RatioType NeutralWinRatio { get; init; }
    public required RatioType CounterHitRatio { get; init; }
    public required RatioType BeneficialTradeRatio { get; init; }
}