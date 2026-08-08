using System.Runtime.CompilerServices;

namespace Shimakaze.Sdk.Inilyn.Text;

/// <summary>
/// 对 <see cref="RootSourceText"/> 的子区间视图，不持有任何字符副本。
/// </summary>
/// <remarks>
/// <para>
/// 行偏移表独立计算（惰性），但字符读取委托给所属的 <see cref="RootSourceText"/>。
/// </para>
/// </remarks>
public sealed class SubSourceText : SourceText
{
    private readonly RootSourceText _root;
    private readonly int _offset;
    private readonly int _length;

    private int[]? _lineStartOffsets;
    private int _lineCount;

    /// <summary>
    /// 创建一个子区间视图。
    /// </summary>
    /// <param name="root">所属的根源文本。</param>
    /// <param name="offset">起始偏移量（相对于根文本）。</param>
    /// <param name="length">字符长度。</param>
    internal SubSourceText(RootSourceText root, int offset, int length)
    {
        _root = root;
        _offset = offset;
        _length = length;
    }

    /// <inheritdoc />
    public override string? FileName => _root.FileName;

    /// <inheritdoc />
    public override int Length => _length;

    /// <inheritdoc />
    public override char this[int index] => _root.GetChar(_offset + index);

    /// <inheritdoc />
    public override ReadOnlySpan<char> Span => _root.GetSpan(_offset, _length);

#if NET5_0_OR_GREATER
    /// <inheritdoc />
    public override SourceText this[Range range]
    {
        get
        {
            (int start, int length) = range.GetOffsetAndLength(_length);
            return new SubSourceText(_root, _offset + start, length);
        }
    } 
#endif

    /// <inheritdoc />
    public override SourceText Substring(int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start + length, _length);
        return new SubSourceText(_root, _offset + start, length);
    }

    /// <inheritdoc />
    public override SourceText Substring(int start)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start, _length);
        return new SubSourceText(_root, _offset + start, _length - start);
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
        ArgumentOutOfRangeException.ThrowIfGreaterThan(position, _length);
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
            return new SubSourceText(_root, _offset, 0);

        int lastLine = startLine + count - 1;
        if (lastLine >= _lineCount)
            lastLine = _lineCount - 1;

        int start = GetLineCore(startLine).Start;
        int end = GetLineCore(lastLine).EndIncludingBreak;
        return new SubSourceText(_root, _offset + start, end - start);
    }

    /// <inheritdoc />
    public override string ToString() => _root.GetSpan(_offset, _length).ToString();

    /// <inheritdoc />
    public override bool Equals(SourceText? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (other is not SubSourceText sub)
            return false;

        return ReferenceEquals(_root, sub._root)
            && _offset == sub._offset
            && _length == sub._length;
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(_root, _offset, _length);

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
                offsets = ComputeLineStarts(_root.GetSpan(_offset, _length));
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
        int endIncludingBreak = lineNumber + 1 < _lineCount ? offsets[lineNumber + 1] : _length;
        return new TextLine(_root, lineNumber, _offset + start, _offset + endIncludingBreak);
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
