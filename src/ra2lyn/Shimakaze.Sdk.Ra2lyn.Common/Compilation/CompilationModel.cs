using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Common.Compilation;

/// <summary>
/// 编译结果。
/// </summary>
public sealed class CompilationResult
{
    /// <summary>
    /// 编译产出的模型。
    /// </summary>
    public required CompilationModel Model { get; init; }

    /// <summary>
    /// 编译诊断（已排序去重）。
    /// </summary>
    public required IReadOnlyList<Diagnostic> Diagnostics { get; init; }
}

/// <summary>
/// 后端编译模型：只包含与源语言无关的 IR。
/// </summary>
public sealed class CompilationModel
{
    /// <summary>
    /// 参与编译的源文件。
    /// </summary>
    public required IReadOnlyList<SourceFile> Sources { get; init; }

    /// <summary>
    /// 实体列表，顺序确定。
    /// </summary>
    public required IReadOnlyList<EntityModel> Entities { get; init; }

    /// <summary>
    /// 引用列表，顺序确定。
    /// </summary>
    public required IReadOnlyList<ReferenceModel> References { get; init; }

    /// <summary>
    /// 编译诊断。
    /// </summary>
    public required IReadOnlyList<Diagnostic> Diagnostics { get; init; }

    /// <summary>
    /// 可达实体集合（由分析阶段给出）；为 <see langword="null"/> 表示未做可达性分析，发射时不做裁剪。
    /// </summary>
    public IReadOnlyList<EntityId>? ReachableEntities { get; init; }
}
