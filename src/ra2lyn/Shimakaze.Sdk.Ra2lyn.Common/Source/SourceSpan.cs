namespace Shimakaze.Sdk.Ra2lyn.Common.Source;

/// <summary>
/// 源文件中的位置范围：使用字符偏移量而非行列号。
/// </summary>
public readonly record struct SourceSpan(string Path, int Start, int End)
{
    /// <summary>
    /// 判断当前范围是否带有有效位置。
    /// </summary>
    public bool IsEmpty
        => Path is null || Start > End;

    /// <summary>
    /// 创建覆盖单个字符的零宽范围。
    /// </summary>
    public static SourceSpan At(string path, int position)
        => new() { Path = path, Start = position, End = position };
}
