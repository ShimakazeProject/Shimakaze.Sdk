using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Shimakaze.Sdk.Preprocessor.Kernel;

/// <summary>
/// 预处理器指令参数
/// </summary>
public sealed class CommandParameter
{
    internal CommandParameter([StringSyntax("Regex")] string match, ParameterInfo parameter)
    {
        Match = match;
        Parameter = parameter;
    }

    /// <summary>
    /// 参数正则匹配
    /// </summary>
    [StringSyntax("Regex")]
    public string Match { get; init; }

    /// <summary>
    /// 参数信息
    /// </summary>
    public ParameterInfo Parameter { get; init; }
}
