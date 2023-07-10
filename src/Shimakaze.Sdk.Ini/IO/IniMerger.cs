using System.Collections;
using System.Diagnostics.CodeAnalysis;

using Shimakaze.Sdk.Ini;

namespace Shimakaze.Sdk.IO.Ini;

/// <summary>
/// Ini 合并器
/// </summary>
public class IniMerger : ISet<IniSection>
{
    /// <summary>
    /// 内部的词典
    /// </summary>
    protected readonly Dictionary<string, IniSection> _cache = new();

    /// <inheritdoc />
    public virtual int Count => _cache.Count;

    /// <inheritdoc />
    public virtual bool IsReadOnly { get; } = false;

    /// <inheritdoc />
    public virtual bool Add(IniSection item)
    {
        if (!_cache.TryGetValue(item.Name, out var section))
            _cache.Add(item.Name, section = new() { Name = item.Name });

        foreach (var kvp in item)
            section[kvp.Key] = kvp.Value;

        return true;
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    void ICollection<IniSection>.Add(IniSection item) => Add(item);

    /// <summary>
    /// 生成Ini
    /// </summary>
    /// <returns> Ini文档对象 </returns>
    public virtual IniDocument Build()
    {
        return new(_cache.Values);
    }

    /// <summary>
    /// 直接写入Ini文件到流
    /// </summary>
    /// <param name="stream"> 流 </param>
    /// <param name="progress"> </param>
    /// <param name="cancellationToken"> </param>
    public virtual async Task BuildAndWriteToAsync(Stream stream, IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        await using IniWriter serializer = new(stream, true);
        await serializer.WriteAsync(new(_cache.Values), progress, cancellationToken);
    }

    /// <inheritdoc />
    public virtual void Clear() => _cache.Clear();

    /// <inheritdoc />
    public virtual bool Contains(IniSection item) => _cache.TryGetValue(item.Name, out _);

    /// <inheritdoc />
    public virtual void CopyTo(IniSection[] array, int arrayIndex) => _cache.Values.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public virtual void ExceptWith(IEnumerable<IniSection> other)
    {
        foreach (var item in other)
            _cache.Remove(item.Name);
    }

    /// <inheritdoc />
    public virtual IEnumerator<IniSection> GetEnumerator() => _cache.Values.GetEnumerator();

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public virtual void IntersectWith(IEnumerable<IniSection> other)
    {
        Clear();
        foreach (var item in other)
            Add(item);
    }

    /// <inheritdoc />
    public virtual bool IsProperSubsetOf(IEnumerable<IniSection> other)
    {
        if (other.Count() <= Count)
            return false;

        var tmp = other.Select(item => item.Name);
        return !_cache.Keys.Any(i => !tmp.Contains(i));
    }

    /// <inheritdoc />
    public virtual bool IsProperSupersetOf(IEnumerable<IniSection> other) => other.Count() < Count && !other.Any(i => !_cache.TryGetValue(i.Name, out _));

    /// <inheritdoc />
    public virtual bool IsSubsetOf(IEnumerable<IniSection> other)
    {
        if (other.Count() < Count)
            return false;

        var tmp = other.Select(item => item.Name);
        return !_cache.Keys.Any(i => !tmp.Contains(i));
    }

    /// <inheritdoc />
    public virtual bool IsSupersetOf(IEnumerable<IniSection> other) => other.Count() <= Count && !other.Any(i => !_cache.TryGetValue(i.Name, out _));

    /// <inheritdoc />
    public virtual bool Overlaps(IEnumerable<IniSection> other)
    {
        var tmp = other.Select(item => item.Name);
        return _cache.Keys.Any(i => tmp.Contains(i));
    }

    /// <inheritdoc />
    public virtual bool Remove(IniSection item) => _cache.Remove(item.Name);

    /// <inheritdoc />
    public virtual bool SetEquals(IEnumerable<IniSection> other)
    {
        if (other.Count() != Count)
            return false;

        var tmp = other.Select(item => item.Name);
        return !_cache.Keys.Any(i => !tmp.Contains(i));
    }

    /// <inheritdoc />
    public virtual void SymmetricExceptWith(IEnumerable<IniSection> other)
    {
        foreach (var item in other)
        {
            if (_cache.TryGetValue(item.Name, out var value))
                Remove(value);
            else
                Add(item);
        }
    }

    /// <inheritdoc />
    public virtual void UnionWith(IEnumerable<IniSection> other)
    {
        foreach (var item in other)
            Add(item);
    }
}