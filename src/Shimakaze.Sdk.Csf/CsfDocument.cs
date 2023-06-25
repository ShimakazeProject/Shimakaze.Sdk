namespace Shimakaze.Sdk.Csf;

/// <summary>
/// Csf Document.
/// </summary>
public record struct CsfDocument
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
        Data = data.ToArray();
        ReCount();
    }

    /// <summary>
    /// Gets or sets metadata.
    /// </summary>
    public CsfMetadata Metadata;

    /// <summary>
    /// Gets or sets datas.
    /// </summary>
    public CsfData[] Data = Array.Empty<CsfData>();

    /// <summary>
    /// ReCount.
    /// </summary>
    public void ReCount()
    {
        foreach (CsfData item in Data)
            item.ReCount();

        Metadata.LabelCount = Data.Length;
        Metadata.StringCount = Data.Select(x => x.StringCount).Sum();
    }
}