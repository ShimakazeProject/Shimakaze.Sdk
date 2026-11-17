using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Common.Compilation;

/// <summary>
/// 编译上下文：一次编译的全部输入与中间产物。
/// </summary>
/// <remarks>
/// 上下文只认识“节、组、键、简单值/引用值”这一层结构，不携带规则、类型或分组语义；
/// 这些语义由外部分析器通过 <see cref="Compilation.AddAnalysis"/> 自行携带。
/// </remarks>
public sealed class CompilationContext
{
    /// <summary>
    /// 初始化编译上下文。
    /// </summary>
    /// <param name="options">编译选项。</param>
    /// <param name="frontends">前端结果，顺序即合并顺序。</param>
    public CompilationContext(CompilationOptions options, IReadOnlyList<FrontendResult> frontends)
    {
        Options = options;
        Frontends = frontends;

        List<SourceFile> sources = [];
        List<EntityModel> entities = [];
        HashSet<string> sourcePaths = new(StringComparer.Ordinal);
        foreach (var frontend in frontends)
        {
            foreach (var source in frontend.Sources)
            {
                if (sourcePaths.Add(source.Path))
                    sources.Add(source);
            }

            entities.AddRange(frontend.Entities);

            foreach (var diagnostic in frontend.Diagnostics)
                Diagnostics.Add(diagnostic);
        }

        Sources = sources;
        Entities = entities;
    }

    /// <summary>
    /// 编译选项。
    /// </summary>
    public CompilationOptions Options { get; }

    /// <summary>
    /// 参与本次编译的前端结果。
    /// </summary>
    public IReadOnlyList<FrontendResult> Frontends { get; }

    /// <summary>
    /// 合并后的源文件列表，路径去重。
    /// </summary>
    public IReadOnlyList<SourceFile> Sources { get; }

    /// <summary>
    /// 当前实体列表，由各 Pass 依次替换。
    /// </summary>
    public IReadOnlyList<EntityModel> Entities { get; private set; }

    /// <summary>
    /// 当前引用列表，由引用相关 Pass 替换。
    /// </summary>
    public IReadOnlyList<ReferenceModel> References { get; private set; } = [];

    /// <summary>
    /// 诊断收集器。
    /// </summary>
    public DiagnosticBag Diagnostics { get; } = new();

    /// <summary>
    /// 取消令牌。
    /// </summary>
    public CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// 替换实体列表。
    /// </summary>
    public void SetEntities(IReadOnlyList<EntityModel> entities)
        => Entities = entities;

    /// <summary>
    /// 替换引用列表。
    /// </summary>
    public void SetReferences(IReadOnlyList<ReferenceModel> references)
        => References = references;

    /// <summary>
    /// 接收分析层给出的可达性结论，供发射阶段做 TreeShaking。
    /// </summary>
    /// <param name="reachable">可达实体标识集合。</param>
    /// <remarks>
    /// 内核模型不保存可达性；这里把它转成实体标识集合，供发射器查询。
    /// </remarks>
    public void ApplyReachability(IEnumerable<EntityId> reachable)
        => ReachableEntities = [.. reachable];

    /// <summary>
    /// 可达实体集合；没有运行可达性分析时为空。
    /// </summary>
    public IReadOnlyList<EntityId> ReachableEntities { get; private set; } = [];
}

/// <summary>
/// 编译选项。
/// </summary>
public sealed class CompilationOptions
{
}
