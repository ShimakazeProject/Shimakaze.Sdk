using Shimakaze.Sdk.Inilyn.Syntax;

namespace Shimakaze.Sdk.Inilyn.Symbols;

/// <summary>
/// INI 符号表，持有所有已注册的符号。
/// </summary>
/// <param name="sourceFileName">来源文件名（可选）。</param>
public sealed class IniSymbolTable(string? sourceFileName = null)
{
    /// <summary>
    /// 来源文件名。
    /// </summary>
    public string? SourceFileName { get; } = sourceFileName;

    /// <summary>
    /// 所有已注册的节符号（键为节名，不区分大小写）。
    /// </summary>
    public Dictionary<string, IniSectionSymbol> Sections { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 全局键符号（无节的顶层键值对）。
    /// </summary>
    public Dictionary<string, IniKeySymbol> GlobalKeys { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 所有 Mixin 引用符号。
    /// </summary>
    public List<IniMixinSymbol> AllMixinRefs { get; } = [];

    /// <summary>
    /// 解析过程中收集的诊断信息。
    /// </summary>
    public List<Diagnostic> Diagnostics { get; } = [];
}
