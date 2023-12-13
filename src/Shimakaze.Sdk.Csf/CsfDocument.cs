namespace Shimakaze.Sdk.Csf;

/// <summary>
/// Csf Document.
/// </summary>
public record class CsfDocument
{
    internal CsfMetadata InternalMetadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfDocument" /> class.
    /// </summary>
    public CsfDocument()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfDocument" /> class.
    /// </summary>
    /// <param name="metadata"> metadata. </param>
    public CsfDocument(CsfMetadata metadata)
      : this(metadata, new List<CsfData>(metadata.LabelCount))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfDocument" /> class.
    /// </summary>
    /// <param name="metadata"> metadata. </param>
    /// <param name="data"> datas. </param>
    public CsfDocument(CsfMetadata metadata, IEnumerable<CsfData> data)
    {
        Metadata = metadata;
        Data = data.ToArray();
        ReCount();
    }

    /// <summary>
    /// Gets or sets metadata.
    /// </summary>
    public CsfMetadata Metadata
    {
        get => InternalMetadata;
        set => InternalMetadata = value;
    }

    /// <summary>
    /// Gets or sets datas.
    /// </summary>
    public CsfData[] Data { get; set; } = [];

    /// <summary>
    /// ReCount.
    /// </summary>
    public void ReCount()
    {
        foreach (CsfData item in Data)
            item.ReCount();

        InternalMetadata.LabelCount = Data.Length;
        InternalMetadata.StringCount = Data.Select(x => x.StringCount).Sum();
    }
}