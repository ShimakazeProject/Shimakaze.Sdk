using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Ini.Ares;

/// <summary>
/// 表示一个INI节
/// </summary>
/// <remarks>
/// 构造一个INISection
/// </remarks>
/// <param name="name">节名</param>
/// <param name="base">继承自</param>
/// <param name="map">节数据 (键值对字典)</param>
public sealed class AresIniSection(string name, string? @base, Dictionary<string, string> map) : IniSection(name, map)
{
    /// <summary>
    /// 继承自
    /// </summary>
    [NotNullIfNotNull(nameof(Base))]
    public string? BaseName { get; internal set; } = @base;

    /// <summary>
    /// 继承自
    /// </summary>
    public AresIniSection? Base { get; internal set; }

    /// <inheritdoc/>
    public override ICollection<string> Keys
        => Base is not null
        ? _data
            .Concat(Base)
            .DistinctBy(i => i.Key)
            .Select(i => i.Key)
            .ToList()
        : base.Keys;

    /// <inheritdoc/>
    public override ICollection<string> Values
        => Base is not null
        ? _data
            .Concat(Base)
            .DistinctBy(i => i.Key)
            .Select(i => i.Value)
            .ToList()
        : base.Values;

    /// <inheritdoc/>
    public override int Count
        => Base is not null
        ? _data
            .Concat(Base)
            .DistinctBy(i => i.Key)
            .Count()
        : base.Count;


    /// <inheritdoc/>
    public override string this[string key]
    {
        get
        {
            if (base.TryGetValue(key, out var value)) return value;
            else if (Base is not null) return Base[key];
            throw new KeyNotFoundException();
        }

        set => base[key] = value;
    }

    /// <inheritdoc cref="AresIniSection(string, string, Dictionary{string, string})" />
    public AresIniSection()
        : this(string.Empty, default, [])
    { }

    /// <inheritdoc cref="AresIniSection(string, string, Dictionary{string, string})" />
    public AresIniSection(string name)
        : this(name, default, [])
    { }
    /// <inheritdoc cref="AresIniSection(string, string, Dictionary{string, string})" />
    public AresIniSection(string name, string? @base)
        : this(name, @base, [])
    { }

    /// <inheritdoc cref="AresIniSection(string, string, Dictionary{string, string})" />
    public AresIniSection(Dictionary<string, string> map)
        : this(string.Empty, default, map)
    { }

    /// <inheritdoc cref="AresIniSection(string, string, Dictionary{string, string})" />
    public AresIniSection(string name, Dictionary<string, string> map)
        : this(name, default, map)
    { }

    /// <inheritdoc/>
    public override bool ContainsKey(string key)
    {
        if (base.ContainsKey(key)) return true;
        else if (Base is not null) return Base.ContainsKey(key);
        return false;
    }

    /// <inheritdoc/>
    public override bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        if (base.TryGetValue(key, out value)) return true;
        else if (Base is not null) return Base.TryGetValue(key, out value);
        return false;
    }

    /// <inheritdoc/>
    public override IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        => Base is not null
        ? _data
            .Concat(Base)
            .DistinctBy(i => i.Key)
            .GetEnumerator()
        : base.GetEnumerator();
}