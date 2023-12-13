#if NETSTANDARD
namespace System.Runtime.InteropServices;
public static class CollectionsMarshal
{
    public static Span<T> AsSpan<T>(List<T>? list) => new([.. list]);
}
#endif