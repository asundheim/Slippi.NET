using Slippi.NET.Stats.Types;
using Slippi.NET.Stats.Utils;
using Slippi.NET.Types;

namespace Slippi.NET.Stats;

public static class OverallStats
{
    public static IList<OverallType> GenerateOverallStats(
        GameStart settings,
        IList<PlayerInput> inputs,
        IList<ConversionType> conversions,
        int playableFrameCount)
    {
        var inputsByPlayer = inputs.ToDictionary(i => i.PlayerIndex);
        var originalConversions = conversions;
        var conversionsByPlayer = conversions.GroupBy(c => c.Moves.Count > 0 ? c.Moves[0].PlayerIndex : -1).ToDictionary(kvp => kvp.Key, kvp => kvp.ToList());
        var conversionsByPlayerOpening = conversionsByPlayer.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GroupBy(c => c.OpeningType).ToDictionary(k => k.Key, k => k.ToList()));

        float gameMinutes = (float)playableFrameCount / 3600;

        return settings.Players.Select(player =>
        {
            int playerIndex = player.PlayerIndex;

            if (!inputsByPlayer.TryGetValue(playerIndex, out var playerInputs))
            {
                playerInputs = new PlayerInput()
                {
                    TriggerInputCount = 0,
                    PlayerIndex = 0,
                    OpponentIndex = 0,
                    JoystickInputCount = 0,
                    CstickInputCount = 0,
                    InputCount = 0,
                    ButtonInputCount = 0
                };
            }

            InputCountsType inputCounts = new InputCountsType()
            {
                Buttons = playerInputs.ButtonInputCount,
                Triggers = playerInputs.TriggerInputCount,
                CStick = playerInputs.CstickInputCount,
                Joystick = playerInputs.JoystickInputCount,
                Total = playerInputs.InputCount,
            };

            int conversionCount = 0;
            int successfulConversionCount = 0;

            var opponentIndices = settings.Players
            .Where(opp =>
            {
                // We want players which aren't ourselves
                if (opp.PlayerIndex == playerIndex)
                {
                    return false;
                }

                return !(settings.IsTeams is not null && settings.IsTeams.Value) || opp.TeamId != player.TeamId;
            })
            .Select(opp => opp.PlayerIndex)
            .ToList();

            float totalDamage = 0;
            int killCount = 0;

            // These are the conversions that we did on our opponents
            foreach (var conversion in originalConversions.Where(c => c.PlayerIndex != playerIndex))
            {
                conversionCount++;

                // We killed the opponent
                if (conversion.DidKill && conversion.LastHitBy == playerIndex)
                {
                    killCount++;
                }

                if (conversion.Moves.Count > 1 && conversion.Moves[0].PlayerIndex == playerIndex)
                {
                    successfulConversionCount++;
                }

                foreach (var move in conversion.Moves)
                {
                    if (move.PlayerIndex == playerIndex)
                    {
                        totalDamage += move.Damage;
                    }
                }
            }

            return new OverallType()
            {
                PlayerIndex = playerIndex,
                InputCounts = inputCounts,
                ConversionCount = conversionCount,
                TotalDamage = totalDamage,
                KillCount = killCount,
                SuccessfulConversions = GetRatio(successfulConversionCount, conversionCount),
                InputsPerMinute = GetRatio(inputCounts.Total, gameMinutes),
                DigitalInputsPerMinute = GetRatio(inputCounts.Buttons, gameMinutes),
                OpeningsPerKill = GetRatio(conversionCount, killCount),
                DamagePerOpening = GetRatio(totalDamage, conversionCount),
                NeutralWinRatio = GetOpeningRatio(conversionsByPlayerOpening, playerIndex, opponentIndices, "neutral-win"),
                CounterHitRatio = GetOpeningRatio(conversionsByPlayerOpening, playerIndex, opponentIndices, "counter-attack"),
                BeneficialTradeRatio = GetBeneficialTradeRatio(conversionsByPlayerOpening, playerIndex, opponentIndices)
            };
        }).ToList();
    }

    private static RatioType GetRatio(int count, int total)
    {
        return new RatioType()
        {
            Count = count,
            Total = total,
            Ratio = total > 0 ? (float)count / total : null,
        };
    }

    private static RatioType GetRatio(float count, int total)
    {
        return new RatioType()
        {
            Count = (int)count,
            Total = total,
            Ratio = total > 0 ? count / total : null,
        };
    }

    private static RatioType GetRatio(float count, float total)
    {
        return new RatioType()
        {
            Count = (int)count,
            Total = (int)total,
            Ratio = total > 0 ? count / total : null,
        };
    }

    private static RatioType GetOpeningRatio(
        Dictionary<int, Dictionary<string, List<ConversionType>>> conversionsByPlayerOpening,
        int playerIndex,
        IList<int> opponentIndices,
        string type)
    {
        IList<ConversionType> openings;
        if (conversionsByPlayerOpening.TryGetValue(playerIndex, out var conversionsByType))
        {
            if (conversionsByType.TryGetValue(type, out var openingByType))
            {
                openings = openingByType;
            }
            else
            {
                openings = [];
            }
        }
        else
        {
            openings = [];
        }

        var opponentOpenings = opponentIndices
            .Select(opponentIndex =>
            {
                if (conversionsByPlayerOpening.TryGetValue(opponentIndex, out var opponentConversionsByType))
                {
                    if (opponentConversionsByType.TryGetValue(type, out var opponentConversions))
                    {
                        return opponentConversions;
                    }
                }

                return [];
            })
            .Aggregate(new List<ConversionType>(), (a, b) => [.. a, .. b]);

        return GetRatio(openings.Count, openings.Count + opponentOpenings.Count);
    }

    private static RatioType GetBeneficialTradeRatio(
        Dictionary<int, Dictionary<string, List<ConversionType>>> conversionsByPlayerOpening,
        int playerIndex,
        IList<int> opponentIndices)
    {
        IList<ConversionType> playerTrades;
        if (conversionsByPlayerOpening.TryGetValue(playerIndex, out var conversionsByType))
        {
            if (conversionsByType.TryGetValue("trade", out var trades))
            {
                playerTrades = trades;
            }
            else
            {
                playerTrades = [];
            }
        }
        else
        {
            playerTrades = [];
        }

        var opponentTrades = opponentIndices
            .Select(opponentIndex =>
            {
                if (conversionsByPlayerOpening.TryGetValue(opponentIndex, out var opponentConversionsByType))
                {
                    if (opponentConversionsByType.TryGetValue("trade", out var opponentTrades))
                    {
                        return opponentTrades;
                    }
                }

                return [];
            })
            .Aggregate(new List<ConversionType>(), (a, b) => [..a, ..b]);

        List<ConversionType> benefitsPlayer = [];

        // Figure out which punishes benefited this player
        var zippedTrades = playerTrades.Zip(opponentTrades);
        foreach (var (playerConversion, opponentConversion) in zippedTrades)
        {
            if (playerConversion is not null && opponentConversion is not null)
            {
                float playerDamage = playerConversion.CurrentPercent - playerConversion.StartPercent;
                float opponentDamage = opponentConversion.CurrentPercent - opponentConversion.StartPercent;

                if (playerConversion.DidKill && !opponentConversion.DidKill)
                {
                    benefitsPlayer.Add(playerConversion);
                }
                else if (playerDamage > opponentDamage)
                {
                    benefitsPlayer.Add(playerConversion);
                }
            }
        }

        return GetRatio(benefitsPlayer.Count, playerTrades.Count);
    }
}
