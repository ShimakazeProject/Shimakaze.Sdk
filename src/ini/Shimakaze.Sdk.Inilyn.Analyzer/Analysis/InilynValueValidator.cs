using System.Text.RegularExpressions;

using Shimakaze.Sdk.Inilyn.Analyzer.RuleSet;

namespace Shimakaze.Sdk.Inilyn.Analyzer.Analysis;

/// <summary>
/// 标量值校验器（内置类型 / 枚举 / 元组 / 外部资源）。
/// </summary>
public static partial class InilynValueValidator
{
    [GeneratedRegex(@"^-?\d+$", RegexOptions.CultureInvariant)]
    private static partial Regex IntRegex();

    [GeneratedRegex(@"^-?\d*\.?\d+$", RegexOptions.CultureInvariant)]
    private static partial Regex FloatRegex();

    [GeneratedRegex(@"^\d+%$", RegexOptions.CultureInvariant)]
    private static partial Regex PercentRegex();

    /// <summary>
    /// 校验一个标量值是否符合类型定义。
    /// </summary>
    /// <param name="ruleSet">规则集。</param>
    /// <param name="type">解析后的值类型。</param>
    /// <param name="value">值。</param>
    /// <param name="externalAssets">外部资源清单（种类 → 合法值集合）。</param>
    /// <returns>是否合法。</returns>
    public static bool IsValidScalar(
        InilynRuleSet ruleSet,
        InilynResolvedValueType type,
        string value,
        IReadOnlyDictionary<string, ISet<string>>? externalAssets = null)
    {
        return type.Kind switch
        {
            InilynValueRefKind.Builtin => IsValidBuiltin(type.BuiltinName!, value),
            InilynValueRefKind.Enum => ruleSet.GetEnum(type.EnumName!)?.Values.Contains(value.Trim()) == true,
            InilynValueRefKind.Tuple => IsValidTuple(ruleSet.GetType(type.TupleName!), value),
            // 外部资源：未提供清单或清单不含该种类时跳过校验（宽松）
            InilynValueRefKind.External => externalAssets is null
                || !externalAssets.TryGetValue(type.ExternalName!, out var set)
                || set.Contains(value.Trim()),
            _ => true, // 引用类型由分析器校验
        };
    }

    /// <summary>
    /// 校验内置类型。
    /// </summary>
    /// <param name="builtin">内置类型名。</param>
    /// <param name="value">值。</param>
    /// <returns>是否合法。</returns>
    public static bool IsValidBuiltin(string builtin, string value)
    {
        string v = value.Trim();
        return builtin.ToLowerInvariant() switch
        {
            "int" => IntRegex().IsMatch(v),
            "float" => FloatRegex().IsMatch(v),
            "percent" => PercentRegex().IsMatch(v),
            "boolean" => IsBoolean(v),
            _ => true, // string
        };
    }

    /// <summary>
    /// 校验元组。
    /// </summary>
    /// <param name="tuple">元组类型。</param>
    /// <param name="value">值。</param>
    /// <returns>是否合法。</returns>
    public static bool IsValidTuple(InilynValueType? tuple, string value)
    {
        if (tuple is null)
        {
            return true;
        }

        string[] parts = value.Split(tuple.Separator, StringSplitOptions.TrimEntries);
        if (parts.Length != tuple.Fields.Count)
        {
            return false;
        }

        for (int i = 0; i < parts.Length; i++)
        {
            string fieldType = tuple.Fields[i].Type;
            if (!IsValidBuiltin(fieldType, parts[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 判断是否为布尔值（YyTt1 = true，NnFf0 = false）。
    /// </summary>
    /// <param name="value">值。</param>
    /// <returns>是否为合法布尔值。</returns>
    public static bool IsBoolean(string value)
    {
        string v = value.Trim();
        if (v.Length == 0)
        {
            return false;
        }

        char c = v[0];
        return c is 'y' or 'Y' or 't' or 'T' or '1'
            or 'n' or 'N' or 'f' or 'F' or '0';
    }

    /// <summary>
    /// 校验一个引用值是否应被豁免（如 <c>&lt;none&gt;</c>、<c>null</c>）。
    /// </summary>
    /// <param name="value">值。</param>
    /// <returns>是否豁免。</returns>
    public static bool IsExemptReferenceValue(string value)
    {
        string v = value.Trim();
        return v.Length == 0
            || string.Equals(v, "<none>", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "none", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "null", StringComparison.OrdinalIgnoreCase);
    }
}
