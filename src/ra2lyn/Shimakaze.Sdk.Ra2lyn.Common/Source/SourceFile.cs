namespace Shimakaze.Sdk.Ra2lyn.Common.Source;

/// <summary>
/// 参与编译的一份源文件。
/// </summary>
/// <param name="Path">源文件的稳定标识（通常是路径）。</param>
/// <param name="Text">源文件全文。</param>
/// <param name="Checksum">可选的内容校验值，供增量编译使用。</param>
public sealed record class SourceFile(string Path, string Text, string? Checksum = null);
