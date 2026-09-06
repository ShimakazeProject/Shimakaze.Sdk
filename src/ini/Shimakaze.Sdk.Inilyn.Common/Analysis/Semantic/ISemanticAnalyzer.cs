using Shimakaze.Sdk.Inilyn.Data.Syntax;

namespace Shimakaze.Sdk.Inilyn.Analysis.Semantic;

/// <summary>
/// 语义分析器接口。分析语法节点的语义信息。
/// </summary>
public interface ISemanticAnalyzer : IAnalyzer
{
    /// <summary>
    /// 分析语法节点的语义信息。
    /// </summary>
    /// <param name="context">分析器上下文。</param>
    /// <param name="nodes">语法节点列表。</param>
    /// <returns>语义分析结果。</returns>
    SemanticAnalysisResult Analyze(AnalyzerContext context, IReadOnlyList<SyntaxNode> nodes);
}

/// <summary>
/// 语义分析输出结果。
/// </summary>
public sealed class SemanticAnalysisResult
{
    /// <summary>
    /// 节的语义信息列表。
    /// </summary>
    public List<SemanticSectionInfo> Sections { get; } = [];

    /// <summary>
    /// 节之间的引用关系列表。
    /// </summary>
    public List<SemanticReference> References { get; } = [];
}

/// <summary>
/// 单个节的语义信息。
/// </summary>
public sealed class SemanticSectionInfo
{
    /// <summary>
    /// 节节点。
    /// </summary>
    public required SectionNode Section { get; init; }

    /// <summary>
    /// 规则组名。
    /// </summary>
    public required string GroupName { get; init; }

    /// <summary>
    /// 节的分类。
    /// </summary>
    public required Data.Semantic.SectionKind Kind { get; init; }

    /// <summary>
    /// 推断的类型名。
    /// </summary>
    public string? TypeName { get; init; }

    /// <summary>
    /// 是否可达（用于 TreeShaking）。
    /// </summary>
    public bool IsReachable { get; init; }
}

/// <summary>
/// 语义引用关系。
/// </summary>
public sealed class SemanticReference
{
    /// <summary>
    /// 来源键值对的键名。
    /// </summary>
    public required string SourceKey { get; init; }

    /// <summary>
    /// 来源节 ID。
    /// </summary>
    public required Guid SourceSectionId { get; init; }

    /// <summary>
    /// 目标节名称。
    /// </summary>
    public required string TargetSectionName { get; init; }

    /// <summary>
    /// 引用类型。
    /// </summary>
    public required Data.Semantic.ReferenceKind Kind { get; init; }
}
