using System;
using System.Globalization;
using Slippi.NET.Types;

namespace Slippi.NET.Utils;

public static class GameTimer
{
    /// <summary>
    /// Converts a frame number to a game timer string based on the timer type and starting timer seconds.
    /// </summary>
    /// <param name="frame">The frame number.</param>
    /// <param name="timerType">The type of timer (e.g., DECREASING or INCREASING).</param>
    /// <param name="startingTimerSeconds">The starting timer seconds (required for DECREASING timer).</param>
    /// <returns>A formatted game timer string (e.g., "mm:ss.SS").</returns>
    public static string FrameToGameTimer(int frame, TimerType timerType, int? startingTimerSeconds)
    {
        if (timerType == TimerType.DECREASING)
        {
            if (startingTimerSeconds is null)
            {
                return "Unknown";
            }

            int centiseconds = (int)Math.Ceiling((((60 - (frame % 60)) % 60) * 99) / 59.0);
            float secondsF = startingTimerSeconds.Value - ((float)frame / 60);

            int minutes = (int)(secondsF / 60);
            int seconds = (int)(secondsF - (minutes * 60));
            var date = new DateTime(1, 1, 1, 0, 
                minute: minutes, 
                second: seconds, 
                millisecond: centiseconds * 10);
            return date.ToString("mm:ss.ff", CultureInfo.InvariantCulture);
        }

        if (timerType == TimerType.INCREASING)
        {
            int centiseconds = (int)Math.Floor(((frame % 60) * 99) / 59.0);
            var date = new DateTime(1, 1, 1, 0, 0, frame / 60, centiseconds * 10);
            return date.ToString("mm:ss.ff", CultureInfo.InvariantCulture);
        }

        return "Infinite";
    }
}