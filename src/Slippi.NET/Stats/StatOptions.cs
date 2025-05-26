using Slippi.NET.Types;

namespace Slippi.NET.Stats;

public record class StatOptions
{
    public bool ProcessOnTheFly { get; set; } = false;

    public int FirstFrame { get; set; } = (int)Frames.FIRST;
}