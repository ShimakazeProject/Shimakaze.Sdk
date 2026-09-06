using System.Globalization;

using LsRange = Shimakaze.LanguageServerProtocol.Model.Range;

using Shimakaze.Sdk.Inilyn.Data.Syntax;
using Shimakaze.Sdk.Inilyn.Model;
using Shimakaze.Sdk.Inilyn.Syntax;

namespace Shimakaze.Sdk.Inilyn.Analysis.RuleValidation;

/// <summary>
/// 节规则验证器，根据规则定义检查节的键值对是否符合类型约束。
/// </summary>
/// <remarks>
/// 初始化 <see cref="SectionRuleValidator"/>。
/// </remarks>
/// <param name="group">规则组。</param>
/// <param name="mode">验证模式。</param>
/// <param name="externalValidators">外部类型验证器集合。</param>
public sealed class SectionRuleValidator(
    RuleGroup group,
    ValidationMode mode = ValidationMode.Loose,
    IEnumerable<IExternalTypeValidator>? externalValidators = null)
{
    private readonly TypeValidator _typeValidator = new(group.Types, mode, externalValidators);


    /// <summary>
    /// 验证单个节的所有键值对。
    /// </summary>
    /// <param name="section">待验证的节节点。</param>
    /// <returns>诊断信息列表。</returns>
    public IReadOnlyList<IniDiagnostic> Validate(SectionNode section)
    {
        if (!group.Definitions.TryGetValue(section.Name, out var definition))
            return [];

        var effectiveKeys = definition.GetEffectiveKeys(group);
        List<IniDiagnostic> diagnostics = [];

        foreach (var kv in section.KeyValues)
        {
            if (kv.Key is null)
                continue;

            if (!effectiveKeys.TryGetValue(kv.Key, out var keyRule))
            {
                continue;
            }

            string? error = _typeValidator.Validate(kv.Value, keyRule.Type, keyRule.List);
            if (error is not null)
            {
                diagnostics.Add(CreateDiagnostic(
                    Diagnostics.INI201,
                    kv.Range,
                    section.Name,
                    kv.Key,
                    error));
            }
        }

        return diagnostics;
    }

    private static IniDiagnostic CreateDiagnostic(DiagnosticDescriptor descriptor, LsRange range, params object[] args) => new()
    {
        Id = Guid.NewGuid(),
        Code = descriptor.Id,
        Message = string.Format(CultureInfo.InvariantCulture, descriptor.MessageFormat, args),
        Severity = descriptor.DefaultSeverity,
        Range = range,
    };
}
