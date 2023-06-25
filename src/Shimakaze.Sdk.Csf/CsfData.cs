namespace Shimakaze.Sdk.Csf;

/// <summary>
/// Csf Data.
/// </summary>
public record struct CsfData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsfData"/> class.
    /// </summary>
    public CsfData()
      : this(string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfData"/> class.
    /// </summary>
    /// <param name="labelName">labelName.</param>
    public CsfData(string labelName)
      : this(labelName, new List<CsfValue>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfData"/> class.
    /// </summary>
    /// <param name="labelName">labelName.</param>
    /// <param name="values">values.</param>
    public CsfData(string labelName, IEnumerable<CsfValue> values)
      : this(CsfConstants.LblFlagRaw, 1, labelName.Length, labelName, values)
    {
        ReCount();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfData"/> class.
    /// </summary>
    /// <param name="identifier">identifier.</param>
    /// <param name="stringCount">stringCount.</param>
    /// <param name="labelNameLength">labelNameLength.</param>
    /// <param name="labelName">labelName.</param>
    /// <param name="values">values.</param>
    public CsfData(int identifier, int stringCount, int labelNameLength, string labelName, IEnumerable<CsfValue> values)
    {
        Identifier = identifier;
        StringCount = stringCount;
        LabelNameLength = labelNameLength;
        LabelName = labelName;
        Values = values.ToArray();
    }

    /// <summary>
    /// Gets or sets identifier.
    /// </summary>
    public int Identifier = CsfConstants.LblFlagRaw;

    /// <summary>
    /// Gets or sets stringCount.
    /// </summary>
    public int StringCount;

    /// <summary>
    /// Gets or sets labelNameLength.
    /// </summary>
    public int LabelNameLength;

    /// <summary>
    /// Gets or sets labelName.
    /// </summary>
    public string LabelName;

    /// <summary>
    /// Gets or sets values.
    /// </summary>
    public CsfValue[] Values;

    /// <summary>
    /// Re Count.
    /// </summary>
    public void ReCount()
    {
        StringCount = Values.Length;
    }
}
