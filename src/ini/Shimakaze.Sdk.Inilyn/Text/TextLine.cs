namespace Shimakaze.Sdk.Inilyn.Text;

/// <summary>
/// 源文本中的一行（对 <see cref="SourceText"/> 的一个只读视图，不持有任何副本）。
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Span"/> / <see cref="Text"/> 仅包含该行内容（不含换行符），
/// 而 <see cref="FullSpan"/> / <see cref="FullText"/> 包含末尾换行符（若有）。
/// </para>
/// </remarks>
public readonly struct TextLine : IEquatable<TextLine>
{
    internal TextLine(SourceText text, int lineNumber, int start, int endIncludingBreak)
    {
        SourceText = text;
        LineNumber = lineNumber;
        Start = start;
        EndIncludingBreak = endIncludingBreak;
    }

    /// <summary>
    /// 该行所属的 <see cref="SourceText"/>。
    /// </summary>
    public SourceText SourceText { get; init; }

    /// <summary>
    /// 0 起始行号。
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// 行内容的起始位置（绝对，从 0 开始）。
    /// </summary>
    public int Start { get; init; }

    /// <summary>
    /// 行内容的结束位置（不含换行符，从 0 开始）。
    /// </summary>
    public int End => EndIncludingBreak - LineBreakLength;

    /// <summary>
    /// 包含换行符在内的行结束位置。
    /// </summary>
    public int EndIncludingBreak { get; init; }

    /// <summary>
    /// 行内容长度（不含换行符）。
    /// </summary>
    public int Length => End - Start;

    /// <summary>
    /// 行内容长度（含换行符）。
    /// </summary>
    public int FullLength => EndIncludingBreak - Start;

    /// <summary>
    /// 行内容（不含换行符）的只读切片。
    /// </summary>
    public ReadOnlySpan<char> Span => SourceText.Span[Start..End];

    /// <summary>
    /// 行内容（含换行符）的只读切片。
    /// </summary>
    public ReadOnlySpan<char> FullSpan => SourceText.Span[Start..EndIncludingBreak];

    /// <summary>
    /// 行内容（不含换行符）。
    /// </summary>
    public SourceText Text => SourceText.Substring(Start, Length);

    /// <summary>
    /// 行内容（含换行符）。
    /// </summary>
    public SourceText FullText => SourceText.Substring(Start, FullLength);

    /// <summary>
    /// 是否以换行符结尾。
    /// </summary>
    public bool EndsInLineBreak => LineBreakLength > 0;

    /// <summary>
    /// 末尾换行符的长度（<c>\r\n</c> 为 2，<c>\r</c> 或 <c>\n</c> 为 1，无换行为 0）。
    /// </summary>
    private int LineBreakLength
    {
        get
        {
            if (EndIncludingBreak <= Start || EndIncludingBreak > SourceText.Length)
                return 0;

            char last = SourceText[EndIncludingBreak - 1];
            if (last != '\r' && last != '\n')
                return 0;

            return last == '\n' && EndIncludingBreak - 2 >= Start && SourceText[EndIncludingBreak - 2] == '\r'
                ? 2
                : 1;
        }
    }

    /// <inheritdoc />
    public bool Equals(TextLine other)
        => ReferenceEquals(SourceText, other.SourceText) && LineNumber == other.LineNumber;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is TextLine other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(SourceText, LineNumber);

    /// <summary>
    /// 返回该行的文本内容（不含换行符）。
    /// </summary>
    public override string ToString() => Text.ToString();

    /// <summary>
    /// 判断两个 <see cref="TextLine"/> 是否相等。
    /// </summary>
    public static bool operator ==(TextLine left, TextLine right) => left.Equals(right);

    /// <summary>
    /// 判断两个 <see cref="TextLine"/> 是否不相等。
    /// </summary>
    public static bool operator !=(TextLine left, TextLine right) => !left.Equals(right);
}
