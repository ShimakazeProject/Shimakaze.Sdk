using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Shimakaze.Sdk.Preprocessor.Kernel;

/// <summary>
/// 预处理器指令
/// </summary>
public sealed class Command
{
    /// <summary>
    /// 指令名
    /// </summary>
    [StringSyntax("Regex")]
    public string Name { get; init; }

    /// <summary>
    /// 指令方法
    /// </summary>
    public MethodInfo Method { get; init; }
    /// <summary>
    /// 指令所在的对象
    /// </summary>
    public Type Type { get; init; }

    /// <summary>
    /// 指令参数
    /// </summary>
    public ImmutableArray<CommandParameter> Parameters { get; init; }

    internal Command(Type type, [StringSyntax("Regex")] string? name, MethodInfo method, ImmutableArray<CommandParameter> parameters)
    {
        Type = type;
        Name = string.IsNullOrEmpty(name) ? method.Name.ToLowerInvariant() : name;
        if (Name.EndsWith("Async"))
            Name = Name[..^5];
        Method = method;
        Parameters = parameters;
        Regex = new(@$"^#{Name}\s*{string.Join(@"\s*", parameters.Select(i => $"({i.Match})"))}$");
    }

    internal Regex Regex { get; init; }

    internal void Execute(object? obj, string line)
    {
        Match match = Regex.Match(line);
        var args = match.Groups.Values.Skip(1).Select(i => i.Value).ToImmutableArray();
        object[] parameters = new object[Parameters.Length];
        for (int i = 0; i < Parameters.Length; i++)
        {
            var parameter = Parameters[i];
            string arg = args[i];
            parameters[i] = parameter.Parameter.ParameterType != typeof(string)
                ? Convert.ChangeType(arg, parameter.Parameter.ParameterType)
                : arg;
        }
        Method.Invoke(obj, parameters);
    }

    internal bool CanExecute(string line)
    {
        Match match = Regex.Match(line);
        return match.Success;
    }
}
