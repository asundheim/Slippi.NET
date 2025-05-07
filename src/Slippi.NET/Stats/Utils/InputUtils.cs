using Slippi.NET.Stats.Types;

namespace Slippi.NET.Stats.Utils;
public static class InputUtils
{
    public static int CountSetBits(int x)
    {
        var count = 0;
        while (x != 0)
        {
            x &= x - 1;
            count++;
        }
        return count;
    }

    public static JoystickRegion GetJoystickRegion(float x, float y)
    {
        if (x >= 0.2875 && y >= 0.2875) return JoystickRegion.NE;
        if (x >= 0.2875 && y <= -0.2875) return JoystickRegion.SE;
        if (x <= -0.2875 && y <= -0.2875) return JoystickRegion.SW;
        if (x <= -0.2875 && y >= 0.2875) return JoystickRegion.NW;
        if (y >= 0.2875) return JoystickRegion.N;
        if (x >= 0.2875) return JoystickRegion.E;
        if (y <= -0.2875) return JoystickRegion.S;
        if (x <= -0.2875) return JoystickRegion.W;
        return JoystickRegion.DZ;
    }
}
