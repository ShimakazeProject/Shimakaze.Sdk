using Shimakaze.Sdk.Analyzer.Common;
using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Rules;
using Shimakaze.Sdk.Ra2lyn.Common.Source;
using Shimakaze.Sdk.Ra2lyn.Common.Types;

namespace Shimakaze.Sdk.Analyzer.IniRule;

/// <summary>
/// INI 规则分析器：按节定义与键规则校验普通实体的属性值类型。
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>实体带类型但没有对应的节定义：报 <see cref="IniRuleDiagnosticCodes.UnknownDefinition"/>。</description></item>
/// <item><description>实体拥有节定义但出现了未声明的键：报 <see cref="IniRuleDiagnosticCodes.UnknownKey"/>。</description></item>
/// <item><description>值不符合键规则声明的类型：报 <see cref="IniRuleDiagnosticCodes.TypeMismatch"/>。</description></item>
/// </list>
/// 只校验 <see cref="EntityKind.Entity"/>；Registry / Global / EnumSection / Unknown 由各自的语义规则负责。
/// </remarks>
public sealed class IniRuleAnalyzer : IAnalyzer
{
    /// <inheritdoc/>
    public string Name => "IniRule";

    /// <inheritdoc/>
    public void Analyze(AnalyzerContext context)
    {
        if (IniRuleAnalyzerOptions.From(context.Options) is not { RuleSet: { } rules } options)
            return;

        foreach (var entity in context.Model.Entities)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (entity.Kind is not EntityKind.Entity)
                continue;

            RuleGroup? group = rules.FindGroup(entity.Group);
            if (group is null)
                continue;

            List<string> types = [.. options.GetTypes(entity)];
            if (entity.TypeName is { Length: > 0 } declared
                && types.Contains(declared, StringComparer.OrdinalIgnoreCase) is false)
            {
                types.Add(declared);
            }

            SectionDefinitionRule? definition = FindFirstDefinition(rules, entity.Group, types);
            if (definition is null)
            {
                context.Report(
                    IniRuleDiagnosticCodes.UnknownDefinition,
                    IniRuleDiagnosticCodes.UnknownDefinitionSeverity,
                    $"实体 '{entity.Name}' 没有已知的节定义",
                    entity.Origin);
                continue;
            }

            Dictionary<string, KeyRule> keys = definition.GetEffectiveKeys(group);
            TypeValidator validator = new(
                RuleSetLookup.GetTypes(rules, entity.Group),
                RuleSetLookup.GetEnums(rules, entity.Group),
                options.ValidationMode,
                options.ExternalValidators);

            foreach (var property in entity.Properties)
            {
                if (keys.TryGetValue(property.Key, out var keyRule) is false)
                {
                    context.Report(
                        IniRuleDiagnosticCodes.UnknownKey,
                        IniRuleDiagnosticCodes.UnknownKeySeverity,
                        $"节 '{definition.Name}' 未声明键 '{property.Key}'",
                        property.Origin);
                    continue;
                }

                string? error = validator.Validate(property.Value, keyRule);
                if (error is not null)
                {
                    context.Report(
                        IniRuleDiagnosticCodes.TypeMismatch,
                        IniRuleDiagnosticCodes.TypeMismatchSeverity,
                        $"节 '{entity.Name}' 的键 '{property.Key}' 类型不匹配: {error}",
                        property.Origin);
                }
            }
        }
    }

    private static SectionDefinitionRule? FindFirstDefinition(RuleSet rules, string group, List<string> types)
    {
        foreach (string type in types)
        {
            if (RuleSetLookup.FindDefinition(rules, group, type) is { } definition)
                return definition;
        }

        return null;
    }
}

/// <summary>
/// INI 规则分析器的诊断代码及其严重级别。
/// </summary>
public static class IniRuleDiagnosticCodes
{
    /// <summary>
    /// INI201：属性值类型不匹配。
    /// </summary>
    public const string TypeMismatch = DiagnosticCodes.TypeMismatch;

    /// <summary>
    /// INI202：实体没有已知的节定义。
    /// </summary>
    public const string UnknownDefinition = DiagnosticCodes.UnknownDefinition;

    /// <summary>
    /// INI203：实体出现未声明的键。
    /// </summary>
    public const string UnknownKey = DiagnosticCodes.UnknownKey;

    /// <summary>
    /// INI201 的严重级别。
    /// </summary>
    public const DiagnosticSeverity TypeMismatchSeverity = DiagnosticSeverity.Warning;

    /// <summary>
    /// INI202 的严重级别。
    /// </summary>
    public const DiagnosticSeverity UnknownDefinitionSeverity = DiagnosticSeverity.Warning;

    /// <summary>
    /// INI203 的严重级别。
    /// </summary>
    public const DiagnosticSeverity UnknownKeySeverity = DiagnosticSeverity.Info;
}
