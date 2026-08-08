namespace Shimakaze.Sdk.Inilyn.Analyzer.RuleSet;

/// <summary>
/// 硬编码枚举（跨组共享）。
/// </summary>
public sealed class InilynEnum
{
    private readonly HashSet<string> _values = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 枚举名。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 合法的枚举值集合（不区分大小写）。
    /// </summary>
    public IReadOnlySet<string> Values => _values;

    /// <summary>
    /// 创建一个枚举。
    /// </summary>
    /// <param name="name">枚举名。</param>
    /// <param name="values">初始值。</param>
    public InilynEnum(string name, IEnumerable<string>? values = null)
    {
        Name = name;
        if (values is not null)
        {
            foreach (string v in values)
            {
                _values.Add(v);
            }
        }
    }

    /// <summary>
    /// 追加枚举值（多平台合并时取并集）。
    /// </summary>
    internal void AddRange(IEnumerable<string> values)
    {
        foreach (string v in values)
        {
            _values.Add(v);
        }
    }
}
