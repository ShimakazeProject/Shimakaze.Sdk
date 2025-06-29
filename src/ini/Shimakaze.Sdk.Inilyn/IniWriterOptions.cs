namespace Shimakaze.Sdk.Inilyn;

/// <summary>
/// INI 写入器选项
/// </summary>
public sealed class IniWriterOptions
{
    /// <summary>
    /// 默认选项
    /// </summary>
    public static IniWriterOptions Default { get; } = new();

    /// <summary>
    /// 最小化输出
    /// </summary>
    public static IniWriterOptions Minimal { get; } = new()
    {
        SpaceBeforeEquals = false,
        SpaceAfterEquals = false,
    };

    /// <summary>
    /// 是否在等号左边添加空格
    /// </summary>
    public bool SpaceBeforeEquals { get; init; } = true;
    /// <summary>
    /// 是否在等号右边添加空格
    /// </summary>
    public bool SpaceAfterEquals { get; init; } = true;
}
