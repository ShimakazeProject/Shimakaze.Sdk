using Draco.Lsp.Model;

namespace Shimakaze.Sdk.Inilyn.Text;

/// <summary>
/// 源文本
/// </summary>
public abstract class SourceText(int[] lineIndexes, DocumentUri uri)
{
    /// <summary>
    /// 文档Uri
    /// </summary>
    public DocumentUri Uri { get; } = uri;

    /// <summary>
    /// 获取根文本
    /// </summary>
    public abstract SourceText Root { get; }

    /// <summary>
    /// 获取文本长度
    /// </summary>
    public abstract int Length { get; }

    /// <summary>
    /// 获取行数
    /// </summary>
    public int LineCount => lineIndexes.Length;

    /// <summary>
    /// 获取相对于 <see cref="Root"/> 的起始索引
    /// </summary>
    public abstract int StartIndex { get; }

    /// <summary>
    /// 获取相对于 <see cref="Root"/> 的结束索引
    /// </summary>
    public abstract int EndIndex { get; }

    /// <summary>
    /// 获取指定字符
    /// </summary>
    /// <param name="index">以 <c>0</c> 为基础的字符索引</param>
    /// <returns></returns>
    public abstract char this[Index index] { get; }

    /// <summary>
    /// 获取指定范围的文本
    /// </summary>
    /// <param name="range">以 <c>0</c> 为基础的字符索引范围</param>
    /// <returns></returns>
    public SourceText this[System.Range range] => new SubSourceText(this, range);

    /// <summary>
    /// 从 <paramref name="content"/> 和 <paramref name="uri"/> 创建 <see cref="SourceText"/>
    /// </summary>
    /// <remarks>
    /// 此 API 用于给不在文件系统中的代码使用
    /// </remarks>
    /// <param name="content">代码原文</param>
    /// <param name="uri">URI</param>
    /// <returns></returns>
    public static SourceText Create(string content, DocumentUri uri) => new FullSourceText(content, uri);

    /// <summary>
    /// 从 <paramref name="uri"/> 创建 <see cref="SourceText"/>
    /// </summary>
    /// <param name="uri">URI</param>
    /// <returns></returns>
    public static SourceText Create(DocumentUri uri)
    {
        if (uri.ToUri() is not { IsFile: true } link)
            throw new ArgumentException("uri 不是一个文件 Uri", nameof(uri));

        var content = File.ReadAllText(link.LocalPath);

        return new FullSourceText(content, uri);
    }

    /// <summary>
    /// 从 <paramref name="path"/> 创建 <see cref="SourceText"/>
    /// </summary>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    public static SourceText Create(string path) => Create(new DocumentUri(path));

    /// <summary>
    /// 从 <paramref name="uri"/> 创建 <see cref="SourceText"/>
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    public static SourceText Create(Uri uri) => Create(DocumentUri.From(uri));

    /// <summary>
    /// 获取 <paramref name="index"/> 位置
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Position GetPosition(Index index)
    {
        var p = index.GetOffset(Length);
        var a = lineIndexes.FirstOrDefault(i => i >= p);
        return unchecked(new()
        {
            Line = (uint)Array.IndexOf(lineIndexes, a),
            Character = (uint)(p - a),
        });
    }

    /// <summary>
    /// 获取 <paramref name="range"/> 范围
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public Draco.Lsp.Model.Range GetRange(System.Range range) => new()
    {
        Start = GetPosition(range.Start),
        End = GetPosition(range.End),
    };

    /// <summary>
    /// 获取指定行的文本
    /// </summary>
    /// <param name="lineIndex">以 <c>0</c> 为基础的行号</param>
    /// <returns></returns>
    public SourceText GetLine(Index lineIndex)
    {
        int l = lineIndex.GetOffset(LineCount);
        int startIndex = lineIndexes[l];
        int endIndex = l + 1 < LineCount
            ? lineIndexes[l + 1]
            : Length;
        return new SubSourceText(this, startIndex..endIndex);
    }

    /// <summary>
    /// 获取指定行范围的所有文本
    /// </summary>
    /// <param name="lineRange">以 <c>0</c> 为基础的行号范围</param>
    /// <returns></returns>
    public SourceText GetLines(System.Range lineRange)
    {
        int startLine = lineRange.Start.GetOffset(LineCount);
        int startIndex = lineIndexes[startLine];

        var endLine = lineRange.End.GetOffset(LineCount);
        int endIndex = endLine + 1 < LineCount
            ? lineIndexes[endLine + 1]
            : Length;

        return new SubSourceText(this, startIndex..endIndex);
    }

    /// <summary>
    /// 获取文本内容
    /// </summary>
    /// <returns></returns>
    public abstract override string ToString();

    /// <summary>
    /// 获取文本内容
    /// </summary>
    /// <returns></returns>
    public abstract ReadOnlySpan<char> AsSpan();

    /// <summary>
    /// 去除首尾空格
    /// </summary>
    /// <returns></returns>
    public SourceText Trim()
    {
        int start = 0;
        int end = 0;
        for (int i = 0; i < Length; i++)
        {
            if (!char.IsWhiteSpace(this[i]))
            {
                start = i;
                break;
            }
        }
        for (int i = Length - 1; i >= 0; i--)
        {
            if (!char.IsWhiteSpace(this[i]))
            {
                end = i;
                break;
            }
        }
        return this[start..end];
    }

    /// <summary>
    /// 判断两个文本是否相等
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public sealed override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj is not SourceText other)
            return false;

        return ToString() == other.ToString();
    }

    /// <summary>
    /// 获取哈希值
    /// </summary>
    /// <returns></returns>
    public sealed override int GetHashCode() => ToString().GetHashCode();

    /// <summary>
    /// 判断两个文本是否相等
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(SourceText? left, SourceText? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;

        return left.ToString() == right.ToString();
    }

    /// <summary>
    /// 判断两个文本是否不相等
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(SourceText? left, SourceText? right)
    {
        if (ReferenceEquals(left, right))
            return false;

        if (left is null || right is null)
            return true;

        return left.ToString() != right.ToString();
    }

    /// <summary>
    /// 转换成字符串
    /// </summary>
    /// <param name="text"></param>
    public static implicit operator string(SourceText text) => text.ToString();

    internal static int[] CalculateLineStarts(ReadOnlySpan<char> text)
    {
        List<int> lineStarts = [0];
        for (int i = 0; i < text.Length; i++)
        {
            switch (text[i])
            {
                // CRLF
                case '\r' when text.Length > i + 2 && text[i + 1] is '\n':
                    i++;
                    lineStarts.Add(i + 1);
                    break;
                case '\r': // CR
                case '\n': // LF
                    lineStarts.Add(i + 1);
                    break;
            }
        }
        return [.. lineStarts];
    }
}
