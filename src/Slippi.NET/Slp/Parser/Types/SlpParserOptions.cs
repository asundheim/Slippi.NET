namespace Slippi.NET.Slp.Parser.Types;

public record class SlpParserOptions
{
    /// <summary>
    /// If strict mode is on, we will do strict validation checking
    /// which could throw errors on invalid data.
    /// Default to false though since probably only real time applications
    /// would care about valid data.
    /// </summary>
    public bool Strict { get; set; } = false;
}
