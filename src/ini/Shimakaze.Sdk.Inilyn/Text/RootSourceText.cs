using System.Runtime.CompilerServices;

namespace Shimakaze.Sdk.Inilyn.Text;

/// <summary>
/// 最顶级的源代码文本，拥有底层字符串的所有权。
/// </summary>
/// <remarks>
/// 由 <see cref="SourceText.Create(string, string?)"/> 或隐式字符串转换创建。
/// 负责惰性计算行偏移表。
/// </remarks>
public sealed class RootSourceText : SourceText
{
    private readonly string _text;
    private readonly string? _fileName;

    private int[]? _lineStartOffsets;
    private int _lineCount;

    /// <summary>
    /// 使用完整文本及可选文件名创建 <see cref="RootSourceText"/>。
    /// </summary>
    /// <param name="text">完整文本内容。</param>
    /// <param name="fileName">关联的文件名，可为 <see langword="null"/>。</param>
    internal RootSourceText(string text, string? fileName)
    {
        ArgumentNullException.ThrowIfNull(text);
        _text = text;
        _fileName = fileName;
    }

    /// <inheritdoc />
    public override string? FileName => _fileName;

    /// <inheritdoc />
    public override int Length => _text.Length;

    /// <inheritdoc />
    public override char this[int index] => _text[index];

    /// <inheritdoc />
    public override ReadOnlySpan<char> Span => _text.AsSpan();

#if NET5_0_OR_GREATER
    /// <inheritdoc />
    public override SourceText this[Range range]
    {
        get
        {
            (int start, int length) = range.GetOffsetAndLength(_text.Length);
            return new SubSourceText(this, start, length);
        }
    } 
#endif

    /// <inheritdoc />
    public override SourceText Substring(int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start + length, _text.Length);
        return new SubSourceText(this, start, length);
    }

    /// <inheritdoc />
    public override SourceText Substring(int start)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start, _text.Length);
        return new SubSourceText(this, start, _text.Length - start);
    }

    /// <inheritdoc />
    public override int LineCount => EnsureLineInfo();

    /// <inheritdoc />
    public override IEnumerable<TextLine> Lines
    {
        get
        {
            int count = EnsureLineInfo();
            for (int i = 0; i < count; i++)
                yield return GetLineCore(i);
        }
    }

    /// <inheritdoc />
    public override int GetLineNumber(int position)
    {
        EnsureLineInfo();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(position, _text.Length);
        return GetLineNumberCore(position) + 1;
    }

    /// <inheritdoc />
    public override TextLine GetLine(int lineNumber)
    {
        EnsureLineInfo();
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(lineNumber, _lineCount, nameof(lineNumber));

        return GetLineCore(lineNumber);
    }

    /// <inheritdoc />
    public override SourceText GetLines(int startLine, int count)
    {
        EnsureLineInfo();
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(startLine, _lineCount, nameof(startLine));

        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (count == 0)
            return new SubSourceText(this, 0, 0);

        int lastLine = startLine + count - 1;
        if (lastLine >= _lineCount)
            lastLine = _lineCount - 1;

        int start = GetLineCore(startLine).Start;
        int end = GetLineCore(lastLine).EndIncludingBreak;
        return new SubSourceText(this, start, end - start);
    }

    /// <inheritdoc />
    public override string ToString() => _text;

    /// <inheritdoc />
    public override bool Equals(SourceText? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (other is not RootSourceText root)
            return false;

        return _text.Equals(root._text, StringComparison.Ordinal)
            && _fileName == root._fileName;
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_text, _fileName);

    /// <summary>
    /// 内部获取字符（供 <see cref="SubSourceText"/> 读取底层字符）。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal char GetChar(int index) => _text[index];

    /// <summary>
    /// 内部获取子字符串的 Span（供 <see cref="SubSourceText"/> 使用）。
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ReadOnlySpan<char> GetSpan(int start, int length)
        => _text.AsSpan(start, length);

    private int EnsureLineInfo()
    {
        int[]? offsets = _lineStartOffsets;
        if (offsets is not null)
            return _lineCount;

        lock (this)
        {
            offsets = _lineStartOffsets;
            if (offsets is null)
            {
                offsets = ComputeLineStarts(_text.AsSpan());
                _lineStartOffsets = offsets;
                _lineCount = offsets.Length;
            }
        }

        return _lineCount;
    }

    private int GetLineNumberCore(int position)
    {
        int[] offsets = _lineStartOffsets ?? throw new InvalidOperationException();
        int lo = 0;
        int hi = _lineCount - 1;
        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            if (offsets[mid] > position)
                hi = mid - 1;
            else
                lo = mid + 1;
        }

        return hi >= 0 ? hi : 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TextLine GetLineCore(int lineNumber)
    {
        int[] offsets = _lineStartOffsets ?? throw new InvalidOperationException();
        int start = offsets[lineNumber];
        int endIncludingBreak = lineNumber + 1 < _lineCount ? offsets[lineNumber + 1] : _text.Length;
        return new TextLine(this, lineNumber, start, endIncludingBreak);
    }

    private static int[] ComputeLineStarts(ReadOnlySpan<char> text)
    {
        int capacity = 4;
        int[] starts = new int[capacity];
        int count = 0;
        starts[count++] = 0;

        int i = 0;
        while (i < text.Length)
        {
            char c = text[i];
            if (c == '\r')
            {
                if (i + 1 < text.Length && text[i + 1] == '\n')
                    i++;

                i++;
            }
            else if (c == '\n')
            {
                i++;
            }
            else
            {
                i++;
                continue;
            }

            if (count == starts.Length)
                Array.Resize(ref starts, starts.Length * 2);

            starts[count++] = i;
        }

        Array.Resize(ref starts, count);
        return starts;
    }
}
