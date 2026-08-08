using Shimakaze.Sdk.Inilyn.Syntax;

namespace Shimakaze.Sdk.Inilyn.Semantic;

/// <summary>
/// INI 语义模型（Mixin 展开后的扁平化结果）。
/// </summary>
public sealed class IniSemanticModel
{
    /// <summary>
    /// 所有展平后的节。
    /// </summary>
    public IReadOnlyList<IniSemanticSection> Sections { get; init; } = [];

    /// <summary>
    /// 全局键值对（无节的顶层键值对）。
    /// </summary>
    public IReadOnlyList<IniSemanticKeyValue> GlobalKeys { get; init; } = [];

    /// <summary>
    /// 语义分析过程中收集的诊断信息。
    /// </summary>
    public IReadOnlyList<Diagnostic> Diagnostics { get; init; } = [];
}
