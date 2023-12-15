using System.Diagnostics;

namespace Shimakaze.Sdk;

[StackTraceHidden]
internal static class StreamAsserts
{
    public static void CanSeek(Stream stream)
    {
        if(!stream.CanSeek)
            throw new NotSupportedException("The Stream cannot support Seek.");
    }
}
