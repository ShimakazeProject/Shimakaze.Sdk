using System.Diagnostics;

namespace Shimakaze.Sdk;

[StackTraceHidden]
internal static class StreamAsserts
{
    public static void CanSeek(Stream stream)
    {
        if (!stream.CanSeek)
            throw new NotSupportedException("The Stream cannot support Seek.");
    }
    internal static void EndOfStream(int i)
    {
        if (i is -1)
            throw new EndOfStreamException("流过早结束");
    }
}