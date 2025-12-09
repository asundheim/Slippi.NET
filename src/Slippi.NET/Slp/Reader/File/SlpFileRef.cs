using Slippi.NET.Slp.Reader.Types;

namespace Slippi.NET.Slp.Reader.File;

public class SlpFileRef : SlpRef, IDisposable
{
    public SlpFileRef(string filePath)
    {
        // 256KB buffer
        FileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096 * 64, FileOptions.Asynchronous);
    }

    public override string Source => SlpInputSource.FILE;
    
    public FileStream FileStream { get; }

    public override int GetLenRef()
    {
        return (int)FileStream.Length;
    }

    public override int ReadRef(Span<byte> buffer, int position)
    {
        FileStream.Seek(position, SeekOrigin.Begin);

        return FileStream.Read(buffer);
    }

    public override void Dispose()
    {
        FileStream.Dispose();
    }
}
