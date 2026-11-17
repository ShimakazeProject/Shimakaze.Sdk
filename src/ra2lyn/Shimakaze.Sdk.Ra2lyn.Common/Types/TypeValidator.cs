using System.Globalization;

using Shimakaze.Sdk.Ra2lyn.Common.Rules;

namespace Shimakaze.Sdk.Ra2lyn.Common.Types;

/// <summary>
/// 值类型验证器：按规则声明的类型校验属性值。
/// </summary>
/// <remarks>
/// 类型解析顺序为：内置类型 → 外部验证器 → 复合类型 → 枚举类型 → 未知类型兜底。
/// </remarks>
public sealed class TypeValidator
{
    private readonly IReadOnlyDictionary<string, TypeDefinition> _typeDefinitions;
    private readonly IReadOnlyDictionary<string, EnumDefinition> _enumDefinitions;
    private readonly Dictionary<string, IExternalTypeValidator> _externalValidators;
    private readonly ValidationMode _mode;

    /// <summary>
    /// 初始化 <see cref="TypeValidator"/>。
    /// </summary>
    /// <param name="typeDefinitions">复合类型定义集合。</param>
    /// <param name="enumDefinitions">枚举定义集合。</param>
    /// <param name="mode">校验模式。</param>
    /// <param name="externalValidators">外部类型验证器集合。</param>
    public TypeValidator(
        IReadOnlyDictionary<string, TypeDefinition> typeDefinitions,
        IReadOnlyDictionary<string, EnumDefinition> enumDefinitions,
        ValidationMode mode = ValidationMode.Loose,
        IEnumerable<IExternalTypeValidator>? externalValidators = null)
    {
        _typeDefinitions = typeDefinitions;
        _enumDefinitions = enumDefinitions;
        _mode = mode;

        Dictionary<string, IExternalTypeValidator> validators = new(StringComparer.OrdinalIgnoreCase);
        foreach (var validator in externalValidators ?? [])
            validators[validator.TypeName] = validator;

        _externalValidators = validators;
    }

    /// <summary>
    /// 按 <see cref="KeyRule"/> 校验属性值；列表类型会逐个校验元素。
    /// </summary>
    public string? Validate(string? value, KeyRule rule)
        => Validate(value, rule.Type, rule.List);

    /// <summary>
    /// 校验属性值；通过时返回 <see langword="null"/>，否则返回错误描述。
    /// </summary>
    /// <param name="value">待校验的值；<see langword="null"/> 视为不校验。</param>
    /// <param name="expectedType">期望的类型名。</param>
    /// <param name="listSeparator">列表分隔符；为 <see langword="null"/> 表示单值。</param>
    public string? Validate(string? value, string expectedType, string? listSeparator)
    {
        if (value is null)
            return null;

        if (listSeparator is not null)
            return ValidateList(value, expectedType, listSeparator);

        return ValidateSingle(value, expectedType);
    }

    /// <summary>
    /// 校验单个值。
    /// </summary>
    /// <param name="value">待校验的值。</param>
    /// <param name="expectedType">期望的类型名。</param>
    public string? ValidateSingle(string value, string expectedType)
        => expectedType.ToLowerInvariant() switch
        {
            "int" => ValidateInt(value),
            "float" => ValidateFloat(value),
            "percent" => ValidatePercent(value),
            "boolean" => ValidateBoolean(value),
            "string" => null,
            _ => ValidateNamedType(value, expectedType),
        };

    private string? ValidateList(string value, string expectedType, string separator)
    {
        string[] items = value.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (string item in items)
        {
            string? error = ValidateSingle(item, expectedType);
            if (error is not null)
                return $"列表元素 '{item}' {error}";
        }

        return null;
    }

    private string? ValidateNamedType(string value, string typeName)
    {
        if (_externalValidators.TryGetValue(typeName, out var externalValidator))
            return externalValidator.Validate(value, _mode);

        if (_typeDefinitions.TryGetValue(typeName, out var typeDefinition))
            return ValidateComposite(value, typeDefinition);

        if (_enumDefinitions.TryGetValue(typeName, out var enumDefinition))
            return ValidateEnum(value, enumDefinition);

        // 未知类型的兜底语义：宽松模式放行（可能是尚未登记的引用类型），严格模式报错。
        return _mode is ValidationMode.Loose
            ? null
            : $"未知类型 '{typeName}'";
    }

    private string? ValidateComposite(string value, TypeDefinition definition)
    {
        if (definition.Separator is null)
        {
            string fieldType = definition.Fields is [var first, ..]
                ? first.Type
                : "string";
            return ValidateSingle(value, fieldType);
        }

        string[] parts = value.Split(definition.Separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != definition.Fields.Count)
            return $"期望 {definition.Fields.Count} 个字段（使用 '{definition.Separator}' 分隔），实际得到 {parts.Length} 个";

        for (int i = 0; i < definition.Fields.Count; i++)
        {
            string? error = ValidateSingle(parts[i], definition.Fields[i].Type);
            if (error is not null)
                return $"字段 '{definition.Fields[i].Name}' {error}";
        }

        return null;
    }

    private static string? ValidateEnum(string value, EnumDefinition definition)
        => definition.Values.Any(item => string.Equals(item, value.Trim(), StringComparison.OrdinalIgnoreCase))
        ? null
        : $"'{value}' 不是枚举 '{definition.Name}' 的取值";

    private static string? ValidateInt(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)
        ? null
        : $"不是有效的整数: '{value}'";

    private static string? ValidateFloat(string value)
        => float.TryParse(value, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _)
        ? null
        : $"不是有效的浮点数: '{value}'";

    private string? ValidatePercent(string value)
        => _mode is ValidationMode.Strict
        ? ValidatePercentStrict(value)
        : ValidatePercentLoose(value);

    private static string? ValidatePercentStrict(string value)
    {
        string numericPart = value.EndsWith('%')
            ? value[..^1]
            : value;

        bool valid = float.TryParse(numericPart, NumberStyles.Float, CultureInfo.InvariantCulture, out float result)
            && result is >= 0f and <= 100f;

        return valid
            ? null
            : $"不是有效的百分比: '{value}'（严格模式要求 0%-100% 或 0-100）";
    }

    private static string? ValidatePercentLoose(string value)
    {
        ReadOnlySpan<char> span = value.AsSpan().TrimStart();
        if (span.Length is 0)
            return "不是有效的百分比: 值为空";

        int start = 0;
        if (span[0] is '+' or '-')
            start = 1;

        int digitCount = 0;
        while (start < span.Length && (char.IsAsciiDigit(span[start]) || span[start] is '.'))
        {
            start++;
            digitCount++;
        }

        return digitCount > 0
            ? null
            : $"不是有效的百分比: '{value}'（宽松模式要求数字开头）";
    }

    private string? ValidateBoolean(string value)
        => _mode is ValidationMode.Strict
        ? ValidateBooleanStrict(value)
        : ValidateBooleanLoose(value);

    private static string? ValidateBooleanStrict(string value)
    {
        ReadOnlySpan<char> span = value.AsSpan().Trim();
        if (bool.TryParse(span, out _))
            return null;

        if (span.Equals("yes", StringComparison.OrdinalIgnoreCase)
            || span.Equals("no", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (int.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue)
            && intValue is 0 or 1)
        {
            return null;
        }

        return $"不是有效的布尔值: '{value}'（严格模式要求 true/false、yes/no 或 1/0）";
    }

    private static string? ValidateBooleanLoose(string value)
        => char.ToLowerInvariant(value.AsSpan().TrimStart()[0]) switch
        {
            't' or 'y' or '1' => null,
            'f' or 'n' or '0' => null,
            _ => $"不是有效的布尔值: '{value}'（宽松模式要求 T/t/Y/y/1 或 F/f/N/n/0 开头）",
        };
}
