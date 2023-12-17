#if NETSTANDARD || NETFRAMEWORK
namespace System.Runtime.InteropServices;
internal static class CollectionsMarshal
{
    public static Span<T> AsSpan<T>(List<T>? list) => new([.. list]);
}
#endif