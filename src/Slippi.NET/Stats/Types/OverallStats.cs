namespace Slippi.NET.Stats.Types;

public record class OverallStats
{
    public required int PlayerIndex { get; init; }
    public required InputCountStats InputCounts { get; init; }
    public required int ConversionCount { get; init; }
    public required float TotalDamage { get; init; }
    public required int KillCount { get; init; }
    public required RatioInfo SuccessfulConversions { get; init; }
    public required RatioInfo InputsPerMinute { get; init; }
    public required RatioInfo DigitalInputsPerMinute { get; init; }
    public required RatioInfo OpeningsPerKill { get; init; }
    public required RatioInfo DamagePerOpening { get; init; }
    public required RatioInfo NeutralWinRatio { get; init; }
    public required RatioInfo CounterHitRatio { get; init; }
    public required RatioInfo BeneficialTradeRatio { get; init; }
}