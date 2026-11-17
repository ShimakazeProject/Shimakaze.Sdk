using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Rules;
using Shimakaze.Sdk.Ra2lyn.Common.Source;
using Shimakaze.Sdk.Ra2lyn.Common.Types;

namespace Shimakaze.Sdk.Ra2lyn.Common.Passes;

/// <summary>
/// 规则校验 Pass：按节定义与键规则校验普通实体的属性值类型。
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>实体类型未知或没有对应的节定义：报 <see cref="DiagnosticCodes.UnknownDefinition"/>。</description></item>
/// <item><description>实体拥有节定义但出现了未声明的键：报 <see cref="DiagnosticCodes.UnknownKey"/>。</description></item>
/// <item><description>值不符合键规则声明的类型：报 <see cref="DiagnosticCodes.TypeMismatch"/>。</description></item>
/// </list>
/// 只校验 <see cref="EntityKind.Entity"/>；Registry / Global / EnumSection / Unknown 由各自的语义规则负责。
/// </remarks>
public sealed class RuleValidationPass : ICompilationPass
{
    /// <inheritdoc/>
    public string Name => "RuleValidation";

    /// <inheritdoc/>
    public void Run(CompilationContext context)
    {
        RuleSet? rules = context.Rules;
        if (rules is null)
            return;

        foreach (var entity in context.Entities)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (entity.Kind is not EntityKind.Entity)
                continue;

            RuleGroup? group = rules.FindGroup(entity.Group);
            if (group is null)
                continue;

            List<string> types = [.. context.GetTypes(entity.Id)];
            if (entity.TypeName is { Length: > 0 } declared && types.Contains(declared, StringComparer.OrdinalIgnoreCase) is false)
                types.Add(declared);

            SectionDefinitionRule? definition = FindFirstDefinition(rules, entity.Group, types);
            if (definition is null)
            {
                context.Diagnostics.Add(new Diagnostic(
                    DiagnosticCodes.UnknownDefinition,
                    DiagnosticSeverity.Warning,
                    $"实体 '{entity.Name}' 没有已知的节定义",
                    entity.Origin));
                continue;
            }

            Dictionary<string, KeyRule> keys = definition.GetEffectiveKeys(group);
            TypeValidator validator = new(
                RuleSetLookup.GetTypes(rules, entity.Group),
                RuleSetLookup.GetEnums(rules, entity.Group),
                context.Options.ValidationMode,
                context.Options.ExternalValidators);

            foreach (var property in entity.Properties)
            {
                if (keys.TryGetValue(property.Key, out var keyRule) is false)
                {
                    context.Diagnostics.Add(new Diagnostic(
                        DiagnosticCodes.UnknownKey,
                        DiagnosticSeverity.Info,
                        $"节 '{definition.Name}' 未声明键 '{property.Key}'",
                        property.Origin));
                    continue;
                }

                string? error = validator.Validate(property.Value, keyRule);
                if (error is not null)
                {
                    context.Diagnostics.Add(new Diagnostic(
                        DiagnosticCodes.TypeMismatch,
                        DiagnosticSeverity.Warning,
                        $"节 '{entity.Name}' 的键 '{property.Key}' 类型不匹配: {error}",
                        property.Origin));
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