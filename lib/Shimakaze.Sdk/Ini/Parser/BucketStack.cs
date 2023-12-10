using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Shimakaze.Sdk.Ini.Parser;

/// <summary>
/// 成对符号栈
/// </summary>
public sealed class BucketStack
{
    private readonly Stack<(char Start, StringBuilder Buffer)> _depths = new();
    private readonly Stack<StringBuilder> _bufferCaches = new();

    /// <summary>
    /// 暂存区
    /// </summary>
    public StringBuilder Buffer { get; private set; } = new();

    /// <summary>
    /// 当前符号
    /// </summary>
    public char Current => _depths.TryPeek(out var result) ? result.Start : char.MinValue;

    /// <summary>
    /// 是否是空的
    /// </summary>
    public bool Empty => _depths.Count is 0;

    /// <summary>
    /// 压栈
    /// </summary>
    /// <param name="start"></param>
    public void Push(char start)
    {
        _depths.Push((start, Buffer));
        if (!_bufferCaches.TryPop(out var buffer))
            buffer = new();
        Buffer = buffer.Clear();
    }

    /// <summary>
    /// 弹栈
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool Pop([NotNullWhen(true)] out string? result)
    {
        result = default;
        (_, StringBuilder lastBuffer) = _depths.Pop();
        if (Buffer is not { Length: 0 })
            result = Buffer.ToString();
        _bufferCaches.Push(Buffer);
        Buffer = lastBuffer;
        return result is not null;
    }
}