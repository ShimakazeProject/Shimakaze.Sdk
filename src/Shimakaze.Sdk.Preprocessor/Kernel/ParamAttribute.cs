using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Preprocessor.Kernel;

/// <summary>
/// 预处理器指令参数
/// </summary>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
public sealed class ParamAttribute : Attribute
{
    /// <summary>
    /// 正则匹配预处理器指令参数
    /// </summary>
    /// <param name="pattern"></param>
    public ParamAttribute([StringSyntax("Regex")] string pattern = @"\w*") => Regex = pattern;

    [StringSyntax("Regex")]
    internal string Regex { get; init; }
}