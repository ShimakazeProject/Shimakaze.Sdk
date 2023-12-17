#if NETSTANDARD2_0 || NETFRAMEWORK
namespace System.IO;

public static class StreamExtensions
{
    public static int Read(this Stream stream, Span<byte> buffer)
    {
        var tmp = buffer.ToArray();
        stream.Read(tmp, 0, tmp.Length);
        tmp.CopyTo(buffer);
        return tmp.Length;
    }

    public static void Write(this Stream stream, ReadOnlySpan<byte> buffer)
    {
        var tmp = buffer.ToArray();
        stream.Write(tmp, 0, tmp.Length);
    }
}

#endif