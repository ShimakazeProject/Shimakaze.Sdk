using System.Runtime.CompilerServices;

namespace Shimakaze.Sdk.Mix.Toolkit;

internal static class MixWriterExtension
{
    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    public static extern void WriteFilesInternal(this MixWriter writer, IEnumerable<MixEntry> entries, IEnumerable<FileInfo> files);
}
