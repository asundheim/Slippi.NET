using Slippi.NET.Melee.Types;

namespace Slippi.NET.Melee.Data;

public static class MoveNames
{
    public static readonly Dictionary<int, Move> Lookup = new()
    {
        { 1, new Move(1, "Miscellaneous", "misc") },
        { 2, new Move(2, "Jab", "jab") },
        { 3, new Move(3, "Jab", "jab") },
        { 4, new Move(4, "Jab", "jab") },
        { 5, new Move(5, "Rapid Jabs", "rapid-jabs") },
        { 6, new Move(6, "Dash Attack", "dash") },
        { 7, new Move(7, "Forward Tilt", "ftilt") },
        { 8, new Move(8, "Up Tilt", "utilt") },
        { 9, new Move(9, "Down Tilt", "dtilt") },
        { 10, new Move(10, "Forward Smash", "fsmash") },
        { 11, new Move(11, "Up Smash", "usmash") },
        { 12, new Move(12, "Down Smash", "dsmash") },
        { 13, new Move(13, "Neutral Air", "nair") },
        { 14, new Move(14, "Forward Air", "fair") },
        { 15, new Move(15, "Back Air", "bair") },
        { 16, new Move(16, "Up Air", "uair") },
        { 17, new Move(17, "Down Air", "dair") },
        { 18, new Move(18, "Neutral B", "neutral-b") },
        { 19, new Move(19, "Side B", "side-b") },
        { 20, new Move(20, "Up B", "up-b") },
        { 21, new Move(21, "Down B", "down-b") },
        { 50, new Move(50, "Getup Attack", "getup") },
        { 51, new Move(51, "Getup Attack (Slow)", "getup-slow") },
        { 52, new Move(52, "Grab Pummel", "pummel") },
        { 53, new Move(53, "Forward Throw", "fthrow") },
        { 54, new Move(54, "Back Throw", "bthrow") },
        { 55, new Move(55, "Up Throw", "uthrow") },
        { 56, new Move(56, "Down Throw", "dthrow") },
        { 61, new Move(61, "Edge Attack (Slow)", "edge-slow") },
        { 62, new Move(62, "Edge Attack", "edge") }
    };
}
