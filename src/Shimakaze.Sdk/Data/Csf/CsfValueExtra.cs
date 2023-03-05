namespace Shimakaze.Sdk.Data.Csf;

/// <summary>
/// Extra Value.
/// </summary>
public record CsfValueExtra : CsfValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsfValueExtra"/> class.
    /// </summary>
    public CsfValueExtra()
      : this(string.Empty, string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfValueExtra"/> class.
    /// </summary>
    /// <param name="value">value.</param>
    /// <param name="extraValue">extraValue.</param>
    public CsfValueExtra(string value, string extraValue)
      : this(CsfConstants.StrwFlgRaw, value.Length, value, extraValue.Length, extraValue)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfValueExtra"/> class.
    /// </summary>
    /// <param name="identifier">identifier.</param>
    /// <param name="valueLength">value length.</param>
    /// <param name="value">value.</param>
    /// <param name="extraValueLength">extra value length.</param>
    /// <param name="extraValue">extraValue.</param>
    public CsfValueExtra(int identifier, int valueLength, string value, int extraValueLength, string extraValue)
      : base(identifier, valueLength, value)
    {
        this.ExtraValueLength = extraValueLength;
        this.ExtraValue = extraValue;
    }

    /// <summary>
    /// Gets or sets extra value length.
    /// </summary>
    public int ExtraValueLength { get; set; }

    /// <summary>
    /// Gets or sets extra value.
    /// </summary>
    public string ExtraValue { get; set; }

    /// <summary>
    /// Convert instance to Normal Value.
    /// </summary>
    /// <returns>Normal Value.</returns>
    public CsfValue ToNormal() => new(this.Identifier, this.ValueLength, this.Value);
}
