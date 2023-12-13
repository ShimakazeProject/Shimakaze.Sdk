namespace Shimakaze.Sdk.Ini;

/// <summary>
/// IniDocument 绑定器配置
/// </summary>
public record class IniDocumentBinderOptions
{
    /// <summary>
    /// 默认配置
    /// </summary>
    public static readonly IniDocumentBinderOptions Default = new()
    {
        Trim = true,
    };

    /// <summary>
    /// Trim 键和值的首尾空格
    /// </summary>
    public bool Trim { get; set; }

    /// <summary>
    /// 节名称使用的比较器
    /// </summary>
    public IEqualityComparer<string>? SectionComparer { get; set; }

    /// <summary>
    /// 节内使用的键的比较器
    /// </summary>
    public IEqualityComparer<string>? KeyComparer { get; set; }
}
