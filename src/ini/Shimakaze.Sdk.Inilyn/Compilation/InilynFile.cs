namespace Shimakaze.Sdk.Inilyn.Compilation;

/// <summary>
/// 待编译的 INI 文件描述。
/// </summary>
/// <param name="filePath">文件路径。</param>
/// <param name="isBase">是否为基准文件（在文件排序时置于最前）。</param>
public sealed class InilynFile(string filePath, bool isBase = false)
{
    /// <summary>
    /// 文件路径。
    /// </summary>
    public string FilePath { get; } = filePath;

    /// <summary>
    /// 文件名（从路径中提取）。
    /// </summary>
    public string FileName { get; } = Path.GetFileName(filePath);

    /// <summary>
    /// 是否为基准文件。为 <see langword="true"/> 时在文件排序中置于最前。
    /// </summary>
    public bool IsBase { get; } = isBase;

    /// <summary>
    /// 创建一个 INI 文件描述。
    /// </summary>
    /// <param name="filePath">文件路径。</param>
    /// <param name="isBase">是否为基准文件。</param>
    /// <returns>文件描述实例。</returns>
    public static InilynFile Create(string filePath, bool isBase = false)
        => new(filePath, isBase);
}
