using Slippi.NET.Slp.Reader.Types;

namespace Slippi.NET.Slp.Reader.Buffer;

public class SlpBufferSourceRef : SlpRef
{
    public override string Source => SlpInputSource.BUFFER;
    
    public required byte[] Buffer { get; set; }

    public override int GetLenRef()
    {
        return Buffer.Length;
    }

    public override int ReadRef(Span<byte> buffer, int position)
    {
        if (position >= Buffer.Length)
        {
            return 0;
        }

        int bytesRead = Math.Min(buffer.Length, Buffer.Length - position);
        Buffer.AsSpan().Slice(position, bytesRead).CopyTo(buffer);

        return bytesRead;
    }
}
