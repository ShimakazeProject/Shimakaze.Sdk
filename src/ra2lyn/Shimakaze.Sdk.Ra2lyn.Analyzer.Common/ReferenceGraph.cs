using Shimakaze.Sdk.Ra2lyn.Common.Ir;

namespace Shimakaze.Sdk.Ra2lyn.Analyzer.Common;

/// <summary>
/// 引用图：分析层解析出的实体引用，供可达性分析与发射阶段使用。
/// </summary>
public sealed class ReferenceGraph
{
    private List<ReferenceModel> _references = [];

    /// <summary>
    /// 当前引用列表，顺序由最后一次 <see cref="SetReferences"/> 决定。
    /// </summary>
    public IReadOnlyList<ReferenceModel> References
        => _references;

    /// <summary>
    /// 替换引用列表；分析器应整体替换而不是逐条追加，保证顺序稳定。
    /// </summary>
    /// <param name="references">引用列表。</param>
    public void SetReferences(IReadOnlyList<ReferenceModel> references)
        => _references = [.. references];
}
