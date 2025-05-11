namespace Slippi.NET.Melee;

public enum DeathDirection 
{ 
    DOWN = 0,
    LEFT = 1,
    RIGHT = 2,
    UP = 3
}

public static class AnimationUtils
{
    /// <summary>
    /// Gets the death direction based on the action state ID.
    /// </summary>
    /// <param name="actionStateId">The action state ID.</param>
    /// <returns>The death direction as a string, or null if the ID is invalid.</returns>
    public static DeathDirection? GetDeathDirection(int actionStateId)
    {
        if (actionStateId > 0xA)
        {
            return null;
        }

        return actionStateId switch
        {
            0 => DeathDirection.DOWN,
            1 => DeathDirection.LEFT,
            2 => DeathDirection.RIGHT,
            _ => DeathDirection.UP
        };
    }
}