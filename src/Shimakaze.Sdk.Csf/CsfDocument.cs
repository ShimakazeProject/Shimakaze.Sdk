using System.Collections;

namespace Shimakaze.Sdk.Csf;

/// <summary>
/// Csf Document.
/// </summary>
public record CsfDocument : IList<CsfData>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsfDocument"/> class.
    /// </summary>
    public CsfDocument()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfDocument"/> class.
    /// </summary>
    /// <param name="metadata">metadata.</param>
    public CsfDocument(CsfMetadata metadata)
      : this(metadata, new List<CsfData>(metadata.LabelCount))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfDocument"/> class.
    /// </summary>
    /// <param name="metadata">metadata.</param>
    /// <param name="data">datas.</param>
    public CsfDocument(CsfMetadata metadata, IEnumerable<CsfData> data)
    {
        Metadata = metadata;
        Data = data.ToList();
        ReCount();
    }

    /// <summary>
    /// Gets or sets metadata.
    /// </summary>
    public CsfMetadata Metadata { get; set; } = default;

    /// <summary>
    /// Gets or sets datas.
    /// </summary>
    public IList<CsfData> Data { get; set; } = new List<CsfData>();

    /// <inheritdoc/>
    public int Count => Data.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => Data.IsReadOnly;

    /// <inheritdoc/>
    public CsfData this[int index] { get => Data[index]; set => Data[index] = value; }

    /// <inheritdoc/>
    public void Add(CsfData item)
    {
        Data.Add(item);
    }

    /// <summary>
    /// Add new data.
    /// </summary>
    /// <param name="label">label.</param>
    /// <param name="values">values.</param>
    public void Add(string label, IEnumerable<CsfValue> values)
    {
        Add(new CsfData(label, values));
    }

    /// <inheritdoc/>
    public void Clear()
    {
        Data.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(CsfData item)
    {
        return Data.Contains(item);
    }

    /// <inheritdoc/>
    public void CopyTo(CsfData[] array, int arrayIndex)
    {
        Data.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public IEnumerator<CsfData> GetEnumerator()
    {
        return Data.GetEnumerator();
    }

    /// <inheritdoc/>
    public int IndexOf(CsfData item)
    {
        return Data.IndexOf(item);
    }

    /// <inheritdoc/>
    public void Insert(int index, CsfData item)
    {
        Data.Insert(index, item);
    }

    /// <summary>
    /// ReCount.
    /// </summary>
    public void ReCount()
    {
        foreach (CsfData item in Data)
        {
            item.ReCount();
        }

        CsfMetadata head = Metadata;
        head.LabelCount = Data.Count;
        head.StringCount = Data.Select(x => x.StringCount).Sum();
    }

    /// <inheritdoc/>
    public bool Remove(CsfData item)
    {
        return Data.Remove(item);
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
        Data.RemoveAt(index);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Data).GetEnumerator();
    }
}
