namespace Slippi.NET.Melee;

public static class AnimationUtils
{
    /// <summary>
    /// Gets the death direction based on the action state ID.
    /// </summary>
    /// <param name="actionStateId">The action state ID.</param>
    /// <returns>The death direction as a string, or null if the ID is invalid.</returns>
    public static string? GetDeathDirection(int actionStateId)
    {
        if (actionStateId > 0xA)
        {
            return null;
        }

        return actionStateId switch
        {
            0 => "down",
            1 => "left",
            2 => "right",
            _ => "up"
        };
    }
}