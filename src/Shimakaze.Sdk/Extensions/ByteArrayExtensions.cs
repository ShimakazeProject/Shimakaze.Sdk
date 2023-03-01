namespace Shimakaze.Sdk;

internal static class ByteArrayExtensions
{
    public static void CheckLength(this byte[] buffer, int length)
    {
        if (buffer.Length < length)
        {
            throw new ArgumentException($"Buffer too Short! need {length}bytes, but it's {buffer.Length}bytes");
        }
    }
}