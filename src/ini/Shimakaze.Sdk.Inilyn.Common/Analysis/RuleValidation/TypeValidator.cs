using System.Globalization;

using Shimakaze.Sdk.Inilyn.Model;

namespace Shimakaze.Sdk.Inilyn.Analysis.RuleValidation;

/// <summary>
/// 值类型验证器，检查 INI 值字符串是否符合声明的类型。
/// 支持宽松/严格两种验证模式，以及外部类型扩展。
/// </summary>
public sealed class TypeValidator
{
    private readonly Dictionary<string, TypeDefinition> _typeDefinitions;
    private readonly Dictionary<string, IExternalTypeValidator> _externalValidators;
    private readonly ValidationMode _mode;

    /// <summary>
    /// 初始化 <see cref="TypeValidator"/>。
    /// </summary>
    /// <param name="typeDefinitions">类型定义字典（键为类型名称）。</param>
    /// <param name="mode">验证模式。</param>
    /// <param name="externalValidators">外部类型验证器集合。</param>
    public TypeValidator(
        Dictionary<string, TypeDefinition> typeDefinitions,
        ValidationMode mode = ValidationMode.Loose,
        IEnumerable<IExternalTypeValidator>? externalValidators = null)
    {
        _typeDefinitions = typeDefinitions;
        _mode = mode;
        _externalValidators = new Dictionary<string, IExternalTypeValidator>(StringComparer.OrdinalIgnoreCase);
        if (externalValidators is not null)
        {
            foreach (var validator in externalValidators)
            {
                _externalValidators[validator.TypeName] = validator;
            }
        }
    }

    /// <summary>
    /// 验证值是否符合指定的类型。
    /// </summary>
    /// <param name="value">待验证的值字符串。</param>
    /// <param name="expectedType">期望的类型名称。</param>
    /// <param name="listSeparator">列表分隔符（如 <c>,</c>），为 <see langword="null"/> 表示不是列表。</param>
    /// <returns>验证结果，<see langword="null"/> 表示通过。</returns>
    public string? Validate(string? value, string expectedType, string? listSeparator)
    {
        if (value is null)
            return null;

        if (listSeparator is not null)
            return ValidateList(value, expectedType, listSeparator);

        return ValidateSingle(value, expectedType);
    }

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

    private string? ValidateSingle(string value, string expectedType) => expectedType.ToLowerInvariant() switch
    {
        "int" => ValidateInt(value),
        "float" => ValidateFloat(value),
        "percent" => ValidatePercent(value),
        "boolean" => ValidateBoolean(value),
        "string" => null,
        _ => ValidateExternalOrComplex(value, expectedType),
    };

    private string? ValidateExternalOrComplex(string value, string typeName)
    {
        if (_externalValidators.TryGetValue(typeName, out var externalValidator))
        {
            return externalValidator.Validate(value, _mode);
        }

        if (_typeDefinitions.TryGetValue(typeName, out var typeDef))
        {
            return ValidateComplexType(value, typeDef);
        }

        return ValidateAsSectionRef(value);
    }

    private string? ValidateComplexType(string value, TypeDefinition typeDef)
    {
        string? separator = typeDef.Separator;
        if (separator is null)
        {
            return ValidateSingle(value, typeDef.Fields.Count > 0 ? typeDef.Fields[0].Type : "string");
        }

        string[] parts = value.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != typeDef.Fields.Count)
        {
            return $"期望 {typeDef.Fields.Count} 个字段（使用 '{separator}' 分隔），实际得到 {parts.Length} 个";
        }

        for (int i = 0; i < typeDef.Fields.Count; i++)
        {
            string? error = ValidateSingle(parts[i], typeDef.Fields[i].Type);
            if (error is not null)
                return $"字段 '{typeDef.Fields[i].Name}' {error}";
        }

        return null;
    }

    private static string? ValidateAsSectionRef(string value)
    {
        if (value.Length == 0)
            return "值为空";

        return null;
    }

    private static string? ValidateInt(string value)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
            return null;

        return $"不是有效的整数: '{value}'";
    }

    private static string? ValidateFloat(string value)
    {
        if (float.TryParse(value, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _))
            return null;

        return $"不是有效的浮点数: '{value}'";
    }

    /// <summary>
    /// 验证 percent 类型。
    /// <list type="bullet">
    ///   <item><description>宽松模式：以 0-100 整数开头的任意字符（如 <c>50%</c>、<c>50.5 something</c>）。</description></item>
    ///   <item><description>严格模式：<c>0%-100%</c> 或 <c>0-100</c>（纯数字或带 % 后缀）。</description></item>
    /// </list>
    /// </summary>
    private string? ValidatePercent(string value)
    {
        string trimmed = value.Trim();

        if (_mode == ValidationMode.Strict)
        {
            return ValidatePercentStrict(trimmed);
        }

        return ValidatePercentLoose(trimmed);
    }

    private static string? ValidatePercentStrict(string value)
    {
        string numericPart = value.EndsWith('%', StringComparison.Ordinal)
            ? value[..^1]
            : value;

        if (float.TryParse(numericPart, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out float result)
            && result >= 0f && result <= 100f)
        {
            return null;
        }

        return $"不是有效的百分比: '{value}'（严格模式要求 0%-100% 或 0-100）";
    }

    private static string? ValidatePercentLoose(string value)
    {
        var span = value.AsSpan().TrimStart();
        if (span.Length == 0)
            return "不是有效的百分比: 值为空";

        int start = 0;
        if (span[0] is '+' or '-')
            start = 1;

        int digitCount = 0;
        while (start < span.Length && (char.IsAsciiDigit(span[start]) || span[start] == '.'))
        {
            start++;
            digitCount++;
        }

        if (digitCount > 0)
            return null;

        return $"不是有效的百分比: '{value}'（宽松模式要求数字开头）";
    }

    /// <summary>
    /// 验证 boolean 类型。
    /// <list type="bullet">
    ///   <item><description>宽松模式：<c>T/t/Y/y/1</c> 开头为 true，<c>F/f/N/n/0</c> 开头为 false。</description></item>
    ///   <item><description>严格模式：<c>true/false</c>、<c>yes/no</c>、<c>1/0</c>。</description></item>
    /// </list>
    /// </summary>
    private string? ValidateBoolean(string value)
    {
        if (_mode == ValidationMode.Strict)
        {
            return ValidateBooleanStrict(value);
        }

        return ValidateBooleanLoose(value);
    }

    private static string? ValidateBooleanStrict(string value)
    {
        if (bool.TryParse(value, out _))
            return null;

        var span = value.AsSpan().Trim();
        if (span.Equals("yes", StringComparison.OrdinalIgnoreCase) || span.Equals("no", StringComparison.OrdinalIgnoreCase))
            return null;

        if (int.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intVal) && (intVal == 0 || intVal == 1))
            return null;

        return $"不是有效的布尔值: '{value}'（严格模式要求 true/false、yes/no 或 1/0）";
    }

    private static string? ValidateBooleanLoose(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "不是有效的布尔值: 值为空";

        char first = char.ToLowerInvariant(value[0]);
        return first switch
        {
            't' or 'y' or '1' => null,
            'f' or 'n' or '0' => null,
            _ => $"不是有效的布尔值: '{value}'（宽松模式要求 T/t/Y/y/1 或 F/f/N/n/0 开头）",
        };
    }
}
