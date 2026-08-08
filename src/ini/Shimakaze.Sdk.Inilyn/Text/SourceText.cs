namespace Shimakaze.Sdk.Inilyn.Text;

/// <summary>
/// 不可变源代码文本的抽象基类。
/// </summary>
/// <remarks>
/// <para>
/// 底层由字符串承载完整内容；实例表示一个「窗口」（起始偏移 + 长度），
/// 切片操作返回新的 <see cref="SubSourceText"/> 视图，不复制字符。
/// </para>
/// </remarks>
public abstract class SourceText : IEquatable<SourceText>
{
    /// <summary>
    /// 关联的文件名；若不存在则为 <see langword="null"/>。
    /// </summary>
    public abstract string? FileName { get; }

    /// <summary>
    /// 文本总长度（字符数）。
    /// </summary>
    public abstract int Length { get; }

    /// <summary>
    /// 文本是否为空。
    /// </summary>
    public virtual bool IsEmpty => Length == 0;

    /// <summary>
    /// 获取指定位置（从 0 开始）的字符。
    /// </summary>
    /// <param name="index">字符位置。</param>
    public abstract char this[int index] { get; }

    /// <summary>
    /// 整个窗口的只读字符视图（不复制）。
    /// </summary>
    public abstract ReadOnlySpan<char> Span { get; }

#if NET5_0_OR_GREATER
    /// <summary>
    /// 按 Range 取子区间，返回新的 <see cref="SourceText"/> 视图。
    /// </summary>
    /// <param name="range">区间。</param>
    public abstract SourceText this[Range range] { get; } 
#endif

    /// <summary>
    /// 从 <paramref name="start"/> 位置提取指定长度文本，返回新的 <see cref="SourceText"/> 视图。
    /// </summary>
    /// <remarks>
    /// 该方法直接创建 <see cref="SubSourceText"/>。
    /// </remarks>
    /// <param name="start">起始位置。</param>
    /// <param name="length">长度。</param>
    public abstract SourceText Substring(int start, int length);

    /// <summary>
    /// 从 <paramref name="start"/> 位置提取到末尾，返回新的 <see cref="SourceText"/> 视图。
    /// </summary>
    /// <param name="start">起始位置。</param>
    public abstract SourceText Substring(int start);

    /// <summary>
    /// 是否包含给定字符。
    /// </summary>
    /// <param name="value">要查找的字符。</param>
    public bool Contains(char value) => Span.IndexOf(value) >= 0;

    /// <summary>
    /// 是否包含给定子字符串。
    /// </summary>
    /// <param name="value">要查找的子字符串。</param>
    public bool Contains(string value) => Span.IndexOf(value) >= 0;

    /// <summary>
    /// 是否以给定值开头。
    /// </summary>
    /// <param name="value">前缀。</param>
    public bool StartsWith(char value) => Span.Length > 0 && Span[0] == value;

    /// <summary>
    /// 是否以给定值开头。
    /// </summary>
    /// <param name="value">前缀。</param>
    public bool StartsWith(string value) => Span.StartsWith(value);

    /// <summary>
    /// 是否以给定值结尾。
    /// </summary>
    /// <param name="value">后缀。</param>
    public bool EndsWith(char value) => Span.Length > 0 && Span[^1] == value;

    /// <summary>
    /// 是否以给定值结尾。
    /// </summary>
    /// <param name="value">后缀。</param>
    public bool EndsWith(string value) => Span.EndsWith(value);

    /// <summary>
    /// 查找第一个匹配字符的位置；未找到返回 -1。
    /// </summary>
    /// <param name="value">要查找的字符。</param>
    public int IndexOf(char value) => Span.IndexOf(value);

    /// <summary>
    /// 查找第一个匹配子字符串的位置；未找到返回 -1。
    /// </summary>
    /// <param name="value">要查找的子字符串。</param>
    public int IndexOf(string value) => Span.IndexOf(value);

    /// <summary>
    /// 查找最后一个匹配字符的位置；未找到返回 -1。
    /// </summary>
    /// <param name="value">要查找的字符。</param>
    public int LastIndexOf(char value) => Span.LastIndexOf(value);

    /// <summary>
    /// 查找最后一个匹配子字符串的位置；未找到返回 -1。
    /// </summary>
    /// <param name="value">要查找的子字符串。</param>
    public int LastIndexOf(string value) => Span.LastIndexOf(value);

    /// <summary>
    /// 文本中某位置所在的 1 起始行号。
    /// </summary>
    /// <param name="position">文本位置。</param>
    /// <returns>行号（1 起始）。</returns>
    public abstract int GetLineNumber(int position);

    /// <summary>
    /// 获取指定行号（0 起始）对应的行。
    /// </summary>
    /// <param name="lineNumber">0 起始行号。</param>
    public abstract TextLine GetLine(int lineNumber);

    /// <summary>
    /// 文本包含的行数。
    /// </summary>
    public abstract int LineCount { get; }

    /// <summary>
    /// 按行拆分得到的全部行。
    /// </summary>
    public abstract IEnumerable<TextLine> Lines { get; }

    /// <summary>
    /// 从 <paramref name="startLine"/> 行（0 起始）开始，连续取 <paramref name="count"/> 行。
    /// </summary>
    /// <param name="startLine">起始行号（0 起始）。</param>
    /// <param name="count">要取的行数。</param>
    public abstract SourceText GetLines(int startLine, int count);

    /// <summary>
    /// 获取整行的文本（含换行符）作为 <see cref="SourceText"/> 视图。
    /// </summary>
    /// <param name="lineNumber">0 起始行号。</param>
    public SourceText GetLineText(int lineNumber) => GetLine(lineNumber).FullText;

    /// <summary>
    /// 获取指定位置的行号和列号。
    /// </summary>
    /// <param name="position">文本位置（0-based）。</param>
    /// <returns>行号（1-based）和列号（1-based）。</returns>
    public (int Line, int Column) GetPosition(int position)
    {
        int line = GetLineNumber(position);
        var lineInfo = GetLine(line - 1);
        int column = position - lineInfo.Start + 1;
        return (line, column);
    }

    /// <summary>
    /// 从字符串创建 <see cref="RootSourceText"/>。
    /// </summary>
    /// <param name="text">文本内容。</param>
    /// <param name="fileName">关联的文件名，可为 <see langword="null"/>。</param>
    public static SourceText Create(string text, string? fileName = null)
        => new RootSourceText(text, fileName);

    /// <summary>
    /// 允许从字符串隐式转换为 <see cref="RootSourceText"/>。
    /// </summary>
    /// <param name="text">文本内容。</param>
    public static implicit operator SourceText(string text) => new RootSourceText(text, null);

    /// <summary>
    /// 允许将 <see cref="SourceText"/> 隐式转换为底层字符串。
    /// </summary>
    /// <param name="source">源代码文本。</param>
    public static implicit operator string(SourceText source) => source.ToString();

    /// <inheritdoc />
    public abstract override string ToString();

    /// <inheritdoc />
    public abstract bool Equals(SourceText? other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as SourceText);

    /// <inheritdoc />
    public abstract override int GetHashCode();

    /// <summary>
    /// 判断两个 <see cref="SourceText"/> 是否相等。
    /// </summary>
    public static bool operator ==(SourceText? left, SourceText? right) => Equals(left, right);

    /// <summary>
    /// 判断两个 <see cref="SourceText"/> 是否不相等。
    /// </summary>
    public static bool operator !=(SourceText? left, SourceText? right) => !Equals(left, right);
}
