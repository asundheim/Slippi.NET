using Slippi.NET.Melee.Data;
using Slippi.NET.Melee.Types;

namespace Slippi.NET.Melee;

public static class MoveUtils
{
    public static readonly Move UnknownMove = new Move(-1, "Unknown Move", "unknown");

    /// <summary>
    /// Gets information about a move based on its ID.
    /// </summary>
    /// <param name="moveId">The ID of the move.</param>
    /// <returns>A <see cref="Move"/> object containing the move ID, name, and short name.</returns>
    public static Move GetMoveInfo(int moveId)
    {
        return MoveNames.Lookup.TryGetValue(moveId, out var moveData) ? moveData : UnknownMove;
    }

    /// <summary>
    /// Gets the short name of a move based on its ID.
    /// </summary>
    /// <param name="moveId">The ID of the move.</param>
    /// <returns>The short name of the move.</returns>
    public static string GetMoveShortName(int moveId)
    {
        return GetMoveInfo(moveId).ShortName;
    }

    /// <summary>
    /// Gets the name of a move based on its ID.
    /// </summary>
    /// <param name="moveId">The ID of the move.</param>
    /// <returns>The name of the move.</returns>
    public static string GetMoveName(int moveId)
    {
        return GetMoveInfo(moveId).Name;
    }
}
