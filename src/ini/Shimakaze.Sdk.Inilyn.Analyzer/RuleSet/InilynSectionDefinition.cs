namespace Shimakaze.Sdk.Inilyn.Analyzer.RuleSet;

/// <summary>
/// 键声明：键名 + 值类型 + 可选分隔符。
/// </summary>
/// <param name="Name">键名。</param>
/// <param name="Type">值类型的原始文本（如 <c>Weapon</c>、<c>Art.ObjectType</c>）。</param>
/// <param name="List">可选分隔符（<c>,</c> 或空格），表示值是开放列表。</param>
public sealed record class InilynKeyDeclaration(string Name, string Type, string? List = null);

/// <summary>
/// 节定义（原"节类型"）：描述一个实体节的键表。
/// </summary>
/// <param name="name">节定义名。</param>
public sealed class InilynSectionDefinition(string name)
{
    private readonly Dictionary<string, InilynKeyDeclaration> _keys = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 节定义名。
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 父节定义名（可选）。
    /// </summary>
    public string? Base { get; internal set; }

    /// <summary>
    /// 直接声明的键表（不含继承）。
    /// </summary>
    public IReadOnlyDictionary<string, InilynKeyDeclaration> Keys => _keys;

    /// <summary>
    /// 合并另一个节定义（多平台时后者覆盖/追加）。
    /// </summary>
    /// <param name="other">另一个节定义。</param>
    internal void MergeFrom(InilynSectionDefinition other)
    {
        if (other.Base is not null)
        {
            Base = other.Base;
        }

        foreach (var (k, v) in other._keys)
        {
            _keys[k] = v;
        }
    }

    /// <summary>
    /// 设置父节定义名。
    /// </summary>
    /// <param name="baseName">父节定义名。</param>
    internal void SetBase(string? baseName)
    {
        Base = baseName;
    }

    /// <summary>
    /// 添加一个键声明。
    /// </summary>
    /// <param name="key">键声明。</param>
    internal void AddKey(InilynKeyDeclaration key)
    {
        _keys[key.Name] = key;
    }

    /// <summary>
    /// 获取键声明。
    /// </summary>
    /// <param name="key">键名。</param>
    /// <returns>键声明，不存在时返回 <see langword="null"/>。</returns>
    public InilynKeyDeclaration? GetKey(string key)
    {
        return _keys.GetValueOrDefault(key);
    }

    /// <summary>
    /// 计算有效键表（合并自身与祖先链上的键，子类覆盖同名键）。
    /// </summary>
    /// <param name="group">所属组（用于沿 Base 链查找祖先定义）。</param>
    /// <returns>有效的键声明字典。</returns>
    public IReadOnlyDictionary<string, InilynKeyDeclaration> GetEffectiveKeys(InilynRuleGroup group)
    {
        Dictionary<string, InilynKeyDeclaration> result = new(StringComparer.OrdinalIgnoreCase);
        List<InilynSectionDefinition> stack = [];
        var current = this;
        while (current is not null)
        {
            stack.Add(current);
            current = current.Base is not null ? group.GetDefinition(current.Base) : null;
        }

        for (int i = stack.Count - 1; i >= 0; i--)
        {
            foreach (var (k, v) in stack[i]._keys)
            {
                result[k] = v;
            }
        }

        return result;
    }
}
