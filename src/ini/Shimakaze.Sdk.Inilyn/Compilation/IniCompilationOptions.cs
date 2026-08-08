namespace Shimakaze.Sdk.Inilyn.Compilation;

/// <summary>
/// 编译选项。
/// </summary>
public sealed class IniCompilationOptions
{
    /// <summary>
    /// 是否启用 TreeShaking（默认 true）。
    /// </summary>
    public bool EnableTreeShaking { get; init; } = true;
}
