namespace Shimakaze.Sdk.Inilyn.Compilation;

/// <summary>
/// 待编译的 INI 文件描述。
/// </summary>
/// <param name="filePath">文件路径。</param>
public sealed class InilynFile(string filePath)
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
    /// 创建一个 INI 文件描述。
    /// </summary>
    /// <param name="filePath">文件路径。</param>
    /// <returns>文件描述实例。</returns>
    public static InilynFile Create(string filePath)
        => new(filePath);
}
