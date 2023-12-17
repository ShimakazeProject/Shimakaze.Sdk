using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Preprocessor.Kernel;

/// <summary>
/// 预处理器指令
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class CommandAttribute : Attribute
{
    /// <summary>
    /// 使用完全小写的方法名作为预处理器指令名
    /// </summary>
    public CommandAttribute() : this(string.Empty) { }
    /// <summary>
    /// 使用自定义正则作为预处理器指令名
    /// </summary>
    /// <param name="name"></param>
    public CommandAttribute([StringSyntax("Regex")] string name) => Name = name;

    [StringSyntax("Regex")]
    internal string Name { get; init; }
}