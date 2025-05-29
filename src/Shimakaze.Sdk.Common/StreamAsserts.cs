using System.Diagnostics;

namespace Shimakaze.Sdk.Common;

/// <summary>
/// 流断言工具
/// </summary>
[StackTraceHidden]
public static class StreamAsserts
{
    /// <summary>
    /// 断言流是否允许Seek
    /// </summary>
    /// <param name="stream"></param>
    /// <exception cref="NotSupportedException"></exception>
    public static void CanSeek(Stream stream)
    {
        if (!stream.CanSeek)
        {
            throw new NotSupportedException("The Stream cannot support Seek.");
        }
    }

    /// <summary>
    /// 断言流是否过早结束
    /// </summary>
    /// <remarks>
    /// i <see langword="is"/> <see langword="-1"/>
    /// </remarks>
    /// <param name="i"></param>
    /// <exception cref="EndOfStreamException"></exception>
    public static void EndOfStream(int i)
    {
        if (i is -1)
        {
            throw new EndOfStreamException("流过早结束");
        }
    }
}
