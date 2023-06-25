using System.Collections;

using Shimakaze.Sdk.Csf;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Shimakaze.Sdk.IO.Csf;

/// <summary>
/// Csf合并器
/// </summary>
public class CsfMerger : ISet<CsfData>
{
    /// <summary>
    /// 内部的词典
    /// </summary>
    protected readonly Dictionary<string, CsfData> _cache = new();

    /// <inheritdoc/>
    public virtual int Count => _cache.Count;

    /// <inheritdoc/>
    public virtual bool IsReadOnly { get; } = false;

    /// <inheritdoc/>
    public virtual bool Add(CsfData item) => _cache.TryAdd(item.LabelName, item);


    /// <inheritdoc/>
    public virtual void Clear() => _cache.Clear();


    /// <inheritdoc/>
    public virtual bool Contains(CsfData item) => _cache.TryGetValue(item.LabelName, out _);

    /// <inheritdoc/>
    public virtual void CopyTo(CsfData[] array, int arrayIndex) => _cache.Values.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public virtual void ExceptWith(IEnumerable<CsfData> other)
    {
        foreach (var item in other)
            _cache.Remove(item.LabelName);
    }

    /// <inheritdoc/>
    public virtual IEnumerator<CsfData> GetEnumerator() => _cache.Values.GetEnumerator();

    /// <inheritdoc/>
    public virtual void IntersectWith(IEnumerable<CsfData> other)
    {
        Clear();
        foreach (var item in other)
            Add(item);
    }

    /// <inheritdoc/>
    public virtual bool IsProperSubsetOf(IEnumerable<CsfData> other) => other.Count() > Count && !_cache.Values.Any(i => !other.Contains(i));

    /// <inheritdoc/>
    public virtual bool IsProperSupersetOf(IEnumerable<CsfData> other) => other.Count() < Count && !other.Any(i => !_cache.TryGetValue(i.LabelName, out _));

    /// <inheritdoc/>
    public virtual bool IsSubsetOf(IEnumerable<CsfData> other) => other.Count() >= Count && !_cache.Values.Any(i => !other.Contains(i));

    /// <inheritdoc/>
    public virtual bool IsSupersetOf(IEnumerable<CsfData> other) => other.Count() <= Count && !other.Any(i => !_cache.TryGetValue(i.LabelName, out _));

    /// <inheritdoc/>
    public virtual bool Overlaps(IEnumerable<CsfData> other) => _cache.Values.Any(i => other.Contains(i));

    /// <inheritdoc/>
    public virtual bool Remove(CsfData item) => _cache.Remove(item.LabelName);

    /// <inheritdoc/>
    public virtual bool SetEquals(IEnumerable<CsfData> other) => other.Count() == Count && !_cache.Values.Any(i => !other.Contains(i));

    /// <inheritdoc/>
    public virtual void SymmetricExceptWith(IEnumerable<CsfData> other)
    {
        foreach (var item in other)
        {
            if (_cache.TryGetValue(item.LabelName, out var value))
                Remove(value);
            else
                Add(item);
        }
    }

    /// <inheritdoc/>
    public virtual void UnionWith(IEnumerable<CsfData> other)
    {
        foreach (var item in other)
            _cache[item.LabelName] = item;
    }

    /// <inheritdoc/>
    [CompilerGenerated]
    void ICollection<CsfData>.Add(CsfData item) => Add(item);

    /// <inheritdoc/>
    [CompilerGenerated]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// 生成Csf
    /// </summary>
    /// <param name="language">语言</param>
    /// <param name="version">版本</param>
    /// <param name="unknown">未知</param>
    /// <returns>Csf文档对象</returns>
    public virtual CsfDocument Build(int language = 0, int version = 3, int unknown = 0)
    {
        return new(new(version, language)
        {
            Unknown = unknown
        }, _cache.Values);
    }

    /// <summary>
    /// 直接写入Csf文件到流
    /// </summary>
    /// <param name="stream">流</param>
    /// <param name="language">语言</param>
    /// <param name="version">版本</param>
    /// <param name="unknown">未知</param>
    public virtual void BuildAndWriteTo(Stream stream, int language = 0, int version = 3, int unknown = 0)
    {
        using CsfWriter writer = new(stream, true);
        writer.Write(Build(language, version, unknown));
    }
}