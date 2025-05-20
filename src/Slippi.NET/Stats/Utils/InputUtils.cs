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
        return (x, y) switch
        {
            (>= 0.2875f, >= 0.2875f) => JoystickRegion.NE,
            (>= 0.2875f, <= -0.2875f) => JoystickRegion.SE,
            (<= -0.2875f, <= -0.2875f) => JoystickRegion.SW,
            (<= -0.2875f, >= 0.2875f) => JoystickRegion.NW,
            (_, >= 0.2875f) => JoystickRegion.N,
            (>= 0.2875f, _) => JoystickRegion.E,
            (_, <= -0.2875f) => JoystickRegion.S,
            ( <= -0.2875f, _) => JoystickRegion.W,
            _ => JoystickRegion.DZ
        };
    }
}
