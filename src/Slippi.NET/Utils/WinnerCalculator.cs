using System.Collections.Generic;
using System.Linq;
using Slippi.NET.Types;

namespace Slippi.NET.Utils;

public static class WinnerCalculator
{
    /// <summary>
    /// Determines the winners of a game based on the game end state, settings, and final post-frame updates.
    /// </summary>
    /// <param name="gameEnd">The game end state.</param>
    /// <param name="settings">The game settings, including players and team information.</param>
    /// <param name="finalPostFrameUpdates">The final post-frame updates for all players.</param>
    /// <returns>A list of placements representing the winners.</returns>
    public static IList<PlacementType> GetWinners(
        GameEndType gameEnd,
        (IList<PlayerType> Players, bool IsTeams) settings,
        IList<PostFrameUpdateType> finalPostFrameUpdates)
    {
        var placements = gameEnd.Placements;
        var gameEndMethod = gameEnd.GameEndMethod;
        var lrasInitiatorIndex = gameEnd.LrasInitiatorIndex;
        var (players, isTeams) = settings;

        if (gameEndMethod == GameEndMethod.NO_CONTEST || gameEndMethod == GameEndMethod.UNRESOLVED)
        {
            // The winner is the person who didn't LRAS
            if (lrasInitiatorIndex.HasValue && players.Count == 2)
            {
                var winnerIndex = players.FirstOrDefault(p => p.PlayerIndex != lrasInitiatorIndex)?.PlayerIndex;
                if (winnerIndex.HasValue)
                {
                    return new List<PlacementType>
                    {
                        new PlacementType
                        {
                            PlayerIndex = winnerIndex.Value,
                            Position = 0
                        }
                    };
                }
            }

            return [];
        }

        if (gameEndMethod == GameEndMethod.TIME && players.Count == 2)
        {
            var nonFollowerUpdates = finalPostFrameUpdates.Where(pfu => pfu.IsFollower is null || !pfu.IsFollower.Value).ToList();
            if (nonFollowerUpdates.Count != players.Count)
            {
                return [];
            }

            var p1 = nonFollowerUpdates[0];
            var p2 = nonFollowerUpdates[1];
            if (p1.StocksRemaining > p2.StocksRemaining)
            {
                return new List<PlacementType>
                {
                    new PlacementType { PlayerIndex = p1.PlayerIndex!.Value, Position = 0 }
                };
            }
            else if (p2.StocksRemaining > p1.StocksRemaining)
            {
                return new List<PlacementType>
                {
                    new PlacementType { PlayerIndex = p2.PlayerIndex!.Value, Position = 0 }
                };
            }

            var p1Health = (int)p1.Percent!.Value;
            var p2Health = (int)p2.Percent!.Value;
            if (p1Health < p2Health)
            {
                return new List<PlacementType>
                {
                    new PlacementType { PlayerIndex = p1.PlayerIndex!.Value, Position = 0 }
                };
            }
            else if (p2Health < p1Health)
            {
                return new List<PlacementType>
                {
                    new PlacementType { PlayerIndex = p2.PlayerIndex!.Value, Position = 0 }
                };
            }

            // If stocks and percents were tied, no winner
            return [];
        }

        var firstPosition = placements.FirstOrDefault(p => p.Position == 0);
        if (firstPosition is null)
        {
            return [];
        }

        var winningTeam = players.FirstOrDefault(p => p.PlayerIndex == firstPosition.PlayerIndex)?.TeamId;
        if (isTeams && winningTeam.HasValue)
        {
            return placements.Where(p =>
            {
                var teamId = players.FirstOrDefault(player => player.PlayerIndex == p.PlayerIndex)?.TeamId;
                return teamId == winningTeam;
            }).ToList();
        }

        return new List<PlacementType> { firstPosition };
    }
}