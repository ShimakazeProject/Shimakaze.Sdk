using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Source;
using Shimakaze.Sdk.Ra2lyn.Common.Rules;
using Shimakaze.Sdk.Ra2lyn.Common.Types;

namespace Shimakaze.Sdk.Ra2lyn.Common.Compilation;

/// <summary>
/// 编译上下文：一次编译的全部输入与中间产物。
/// </summary>
public sealed class CompilationContext
{

    /// <summary>
    /// 初始化编译上下文。
    /// </summary>
    /// <param name="options">编译选项。</param>
    /// <param name="frontends">前端结果，顺序即合并顺序。</param>
    /// <param name="rules">规则集；为 <see langword="null"/> 表示不做分类。</param>
    public CompilationContext(CompilationOptions options, IReadOnlyList<FrontendResult> frontends, RuleSet? rules)
    {
        Options = options;
        Frontends = frontends;
        Rules = rules;

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
    /// 规则集；为 <see langword="null"/> 表示不做分类与规则校验。
    /// </summary>
    public RuleSet? Rules { get; }

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
    /// 实体的类型标注：一个实体可同时拥有多个类型（注册表匹配与发现规则追加）。
    /// </summary>
    public Dictionary<EntityId, List<string>> TypeAnnotations { get; } = [];

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
    /// 记录实体拥有的类型；重复类型会被忽略。
    /// </summary>
    public void AnnotateType(EntityId entity, string typeName)
        => AddType(TypeAnnotations, entity, typeName);

    /// <summary>
    /// 获取实体拥有的全部类型；没有标注时返回空列表。
    /// </summary>
    public IReadOnlyList<string> GetTypes(EntityId entity)
        => TypeAnnotations.TryGetValue(entity, out var types) ? types : [];

    /// <summary>
    /// 在实体集合中按组名与实体名查找实体。
    /// </summary>
    public EntityModel? FindEntity(string group, string name, IEnumerable<EntityModel>? entities = null)
        => FindEntityIn(entities ?? Entities, group, name);

    internal static void AddType(Dictionary<EntityId, List<string>> annotations, EntityId entity, string typeName)
    {
        if (!annotations.TryGetValue(entity, out var types))
        {
            types = [];
            annotations[entity] = types;
        }

        if (types.Contains(typeName, StringComparer.OrdinalIgnoreCase) is false)
            types.Add(typeName);
    }

    internal static EntityModel? FindEntityIn(IEnumerable<EntityModel> entities, string group, string name)
    {
        foreach (var entity in entities)
        {
            if (string.Equals(entity.Group, group, StringComparison.OrdinalIgnoreCase)
                && string.Equals(entity.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return entity;
            }
        }

        return null;
    }
}

/// <summary>
/// 编译选项。
/// </summary>
public sealed record class CompilationOptions
{
    /// <summary>
    /// 值校验模式。
    /// </summary>
    public ValidationMode ValidationMode { get; init; } = ValidationMode.Loose;

    /// <summary>
    /// 外部类型验证器集合。
    /// </summary>
    public IReadOnlyList<IExternalTypeValidator> ExternalValidators { get; init; } = [];

    /// <summary>
    /// 前端结果是否已经完成合并（由前端直接给出的多文档 IR）。
    /// </summary>
    public bool IsMergedFrontend { get; init; }
}
