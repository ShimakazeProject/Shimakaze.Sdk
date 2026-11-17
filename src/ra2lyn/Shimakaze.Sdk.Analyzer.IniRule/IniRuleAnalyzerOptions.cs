using Shimakaze.Sdk.Analyzer.Common;
using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Rules;
using Shimakaze.Sdk.Ra2lyn.Common.Types;

namespace Shimakaze.Sdk.Analyzer.IniRule;

/// <summary>
/// 规则分析器选项：在通用分析选项上携带规则集、类型标注与校验模式。
/// </summary>
/// <remarks>
/// <see cref="AnalyzerOptions"/> 是通用的，无法表达“规则校验需要哪些输入”，因此规则分析器使用本类型。
/// 从通用选项转换时会丢失这些字段，此时规则分析器直接跳过而不是报错。
/// </remarks>
public sealed class IniRuleAnalyzerOptions : AnalyzerOptions
{
    /// <summary>
    /// 规则集；为 <see langword="null"/> 时规则分析器不做任何校验。
    /// </summary>
    public RuleSet? RuleSet { get; init; }

    /// <summary>
    /// 实体的类型标注（一个实体可拥有多个类型）；为 <see langword="null"/> 表示只按 <see cref="EntityModel.TypeName"/> 查找。
    /// </summary>
    public IReadOnlyDictionary<EntityId, List<string>>? TypeAnnotations { get; init; }

    /// <summary>
    /// 值校验模式。
    /// </summary>
    public ValidationMode ValidationMode { get; init; } = ValidationMode.Loose;

    /// <summary>
    /// 外部类型验证器集合。
    /// </summary>
    public IReadOnlyList<IExternalTypeValidator> ExternalValidators { get; init; } = [];

    /// <summary>
    /// 把通用选项转成本类型；已经是本类型时原样返回。
    /// </summary>
    /// <param name="options">通用分析选项。</param>
    public static IniRuleAnalyzerOptions? From(AnalyzerOptions options)
        => options as IniRuleAnalyzerOptions;

    /// <summary>
    /// 获取实体拥有的全部类型标注。
    /// </summary>
    /// <param name="entity">实体。</param>
    public IReadOnlyList<string> GetTypes(EntityModel entity)
        => TypeAnnotations is not null && TypeAnnotations.TryGetValue(entity.Id, out var types) ? types : [];

    /// <summary>
    /// 从编译上下文构造选项：规则集、类型标注、校验模式与外部验证器都取自编译结果。
    /// </summary>
    /// <param name="context">编译上下文。</param>
    public static IniRuleAnalyzerOptions FromCompilation(CompilationContext context)
        => new()
        {
            RuleSet = context.Rules,
            TypeAnnotations = context.TypeAnnotations,
            ValidationMode = context.Options.ValidationMode,
            ExternalValidators = context.Options.ExternalValidators,
        };
}
