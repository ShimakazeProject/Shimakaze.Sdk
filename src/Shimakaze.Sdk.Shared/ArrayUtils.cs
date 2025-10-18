namespace Shimakaze.Sdk;

/// <summary>
/// 数组工具
/// </summary>
internal static class ArrayUtils
{
    extension(Array a)
    {
        public static byte[] FastCreate(int length) => FastCreate<byte>(length);

        public static T[] FastCreate<T>(int length)
        {
#if NETSTANDARD
            return new T[length];
#else
            return GC.AllocateUninitializedArray<T>(length);
#endif
        }
    }
}
