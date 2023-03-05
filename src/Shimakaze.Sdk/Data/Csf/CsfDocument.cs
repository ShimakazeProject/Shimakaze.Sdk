using System.Collections;

namespace Shimakaze.Sdk.Data.Csf;

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
        this.Metadata = metadata;
        this.Data = data.ToList();
    }

    /// <summary>
    /// Gets or sets metadata.
    /// </summary>
    public CsfMetadata Metadata { get; set; } = default(CsfMetadata);

    /// <summary>
    /// Gets or sets datas.
    /// </summary>
    public IList<CsfData> Data { get; set; } = new List<CsfData>();

    /// <inheritdoc/>
    public int Count => this.Data.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => this.Data.IsReadOnly;

    /// <inheritdoc/>
    public CsfData this[int index] { get => this.Data[index]; set => this.Data[index] = value; }

    /// <inheritdoc/>
    public void Add(CsfData item)
    {
        this.Data.Add(item);
    }

    /// <summary>
    /// Add new data.
    /// </summary>
    /// <param name="label">label.</param>
    /// <param name="values">values.</param>
    public void Add(string label, IEnumerable<CsfValue> values)
    {
        this.Add(new CsfData(label, values));
    }

    /// <inheritdoc/>
    public void Clear()
    {
        this.Data.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(CsfData item)
    {
        return this.Data.Contains(item);
    }

    /// <inheritdoc/>
    public void CopyTo(CsfData[] array, int arrayIndex)
    {
        this.Data.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public IEnumerator<CsfData> GetEnumerator()
    {
        return this.Data.GetEnumerator();
    }

    /// <inheritdoc/>
    public int IndexOf(CsfData item)
    {
        return this.Data.IndexOf(item);
    }

    /// <inheritdoc/>
    public void Insert(int index, CsfData item)
    {
        this.Data.Insert(index, item);
    }

    /// <summary>
    /// ReCount.
    /// </summary>
    public void ReCount()
    {
        foreach (CsfData item in this.Data)
        {
            item.ReCount();
        }

        CsfMetadata head = this.Metadata;
        head.LabelCount = this.Data.Count;
        head.StringCount = this.Data.Select(x => x.StringCount).Sum();
    }

    /// <inheritdoc/>
    public bool Remove(CsfData item)
    {
        return this.Data.Remove(item);
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
        this.Data.RemoveAt(index);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)this.Data).GetEnumerator();
    }
}
