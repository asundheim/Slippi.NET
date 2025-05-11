namespace Slippi.NET.Slp.Reader.Types;

/// <summary>
/// Contains a set of valid values for <see cref="SlpReader.Source"/>
/// </summary>
/// <remarks>
/// This is mostly for JS SDK compatibility. Callers may simply wish to QI <see cref="SlpReader"/>.
/// </remarks>
public static class SlpInputSource
{
    public const string BUFFER = "buffer";
    public const string FILE = "file";
}
