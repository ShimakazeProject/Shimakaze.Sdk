using System.Collections;

namespace Shimakaze.Sdk.Csf;

/// <summary>
/// Csf合并器
/// </summary>
public class CsfSet
    : ISet<CsfData>
{
    /// <summary>
    /// 内部的词典
    /// </summary>
    protected Dictionary<string, CsfData> Cache { get; } = [];

    /// <inheritdoc />
    public virtual int Count => Cache.Count;

    /// <inheritdoc />
    public virtual bool IsReadOnly { get; }

    /// <inheritdoc />
    public virtual bool Add(CsfData item) => Cache.TryAdd(item.LabelName, item);

    /// <inheritdoc />
    void ICollection<CsfData>.Add(CsfData item) => Add(item);

    /// <summary>
    /// 生成Csf
    /// </summary>
    /// <param name="language"> 语言 </param>
    /// <param name="version"> 版本 </param>
    /// <param name="unknown"> 未知 </param>
    /// <returns> Csf文档对象 </returns>
    public virtual CsfDocument Build(int language = 0, int version = 3, int unknown = 0)
    {
        return new(new(version, language)
        {
            Unknown = unknown
        }, Cache.Values);
    }

    /// <summary>
    /// 直接写入Csf文件到流
    /// </summary>
    /// <param name="stream"> 流 </param>
    /// <param name="language"> 语言 </param>
    /// <param name="version"> 版本 </param>
    /// <param name="unknown"> 未知 </param>
    /// <param name="progress"> 进度 </param>
    public virtual void BuildAndWriteTo(Stream stream, int language = 0, int version = 3, int unknown = 0, IProgress<float>? progress = default)
    {
        CsfWriter.Write(stream, Build(language, version, unknown), progress);
    }

    /// <inheritdoc />
    public virtual void Clear() => Cache.Clear();

    /// <inheritdoc />
    public virtual bool Contains(CsfData item) => Cache.TryGetValue(item.LabelName, out _);

    /// <inheritdoc />
    public virtual void CopyTo(CsfData[] array, int arrayIndex) => Cache.Values.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public virtual void ExceptWith(IEnumerable<CsfData> other)
    {
        foreach (var item in other)
            Cache.Remove(item.LabelName);
    }

    /// <inheritdoc />
    public virtual IEnumerator<CsfData> GetEnumerator() => Cache.Values.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public virtual void IntersectWith(IEnumerable<CsfData> other)
    {
        Clear();
        foreach (var item in other)
            Add(item);
    }

    /// <inheritdoc />
    public virtual bool IsProperSubsetOf(IEnumerable<CsfData> other) => other.Count() > Count && !Cache.Values.Any(i => !other.Contains(i));

    /// <inheritdoc />
    public virtual bool IsProperSupersetOf(IEnumerable<CsfData> other) => other.Count() < Count && !other.Any(i => !Cache.TryGetValue(i.LabelName, out _));

    /// <inheritdoc />
    public virtual bool IsSubsetOf(IEnumerable<CsfData> other) => other.Count() >= Count && !Cache.Values.Any(i => !other.Contains(i));

    /// <inheritdoc />
    public virtual bool IsSupersetOf(IEnumerable<CsfData> other) => other.Count() <= Count && !other.Any(i => !Cache.TryGetValue(i.LabelName, out _));

    /// <inheritdoc />
    public virtual bool Overlaps(IEnumerable<CsfData> other) => Cache.Values.Any(i => other.Contains(i));

    /// <inheritdoc />
    public virtual bool Remove(CsfData item) => Cache.Remove(item.LabelName);

    /// <inheritdoc />
    public virtual bool SetEquals(IEnumerable<CsfData> other) => other.Count() == Count && !Cache.Values.Any(i => !other.Contains(i));

    /// <inheritdoc />
    public virtual void SymmetricExceptWith(IEnumerable<CsfData> other)
    {
        foreach (var item in other)
        {
            if (Cache.TryGetValue(item.LabelName, out var value))
                Remove(value);
            else
                Add(item);
        }
    }

    /// <inheritdoc />
    public virtual void UnionWith(IEnumerable<CsfData> other)
    {
        foreach (var item in other)
            Cache[item.LabelName] = item;
    }
}