namespace Shimakaze.Sdk.Ini.Ares;

/// <summary>
/// IniDocument 绑定器配置
/// </summary>
public record class AresIniDocumentBinderOptions : IniDocumentBinderOptions
{
    /// <summary>
    /// 默认配置
    /// </summary>
    public static new readonly AresIniDocumentBinderOptions Default = new()
    {
        Trim = true,
    };
}