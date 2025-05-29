using System.Collections;

namespace Shimakaze.Sdk.Csf;

/// <summary>
/// CSF 标签
/// </summary>
public record class CsfLabel(string Name, List<CsfValue> Values) : IList<CsfValue>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    public CsfLabel(string name) : this(name, [])
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="capacity"></param>
    public CsfLabel(string name, int capacity) : this(name, Values: new(capacity))
    {
    }

    /// <inheritdoc/>
    public CsfValue this[int index]
    {
        get => Values[index];
        set => Values[index] = value;
    }

    /// <inheritdoc/>
    public string Name { get; set; } = Name;

    /// <inheritdoc/>
    public int Count => Values.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public void Add(CsfValue item) => Values.Add(item);

    /// <inheritdoc/>
    public void Add(string value, string? extra = default) => Add(new(value, extra));

    /// <inheritdoc/>
    public void Clear() => Values.Clear();

    /// <inheritdoc/>
    public bool Contains(CsfValue item) => Values.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(CsfValue[] array, int arrayIndex) => Values.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public IEnumerator<CsfValue> GetEnumerator() => Values.GetEnumerator();

    /// <inheritdoc/>
    public int IndexOf(CsfValue item) => Values.IndexOf(item);

    /// <inheritdoc/>
    public void Insert(int index, CsfValue item) => Values.Insert(index, item);

    /// <inheritdoc/>
    public void Insert(int index, string value, string? extra = default) => Insert(index, new(value, extra));

    /// <inheritdoc/>
    public bool Remove(CsfValue item) => Values.Remove(item);

    /// <inheritdoc/>
    public void RemoveAt(int index) => Values.RemoveAt(index);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
