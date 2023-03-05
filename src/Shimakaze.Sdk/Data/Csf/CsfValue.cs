namespace Shimakaze.Sdk.Data.Csf;

/// <summary>
/// Csf Value.
/// </summary>
public record CsfValue
{
    private static readonly WeakReference<CsfValue> WeakReference = new(new(string.Empty));

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfValue"/> class.
    /// </summary>
    public CsfValue()
      : this(string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfValue"/> class.
    /// </summary>
    /// <param name="value">value.</param>
    public CsfValue(string value)
      : this(CsfConstants.StrFlagRaw, value.Length, value)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfValue"/> class.
    /// </summary>
    /// <param name="identifier">identifier.</param>
    /// <param name="valueLength">value length.</param>
    /// <param name="value">value.</param>
    public CsfValue(int identifier, int valueLength, string value)
    {
        Identifier = identifier;
        ValueLength = valueLength;
        Value = value;
    }

    /// <summary>
    /// Gets Empty.
    /// </summary>
    public static CsfValue Empty
    {
        get
        {
            if (!WeakReference.TryGetTarget(out CsfValue? converter))
            {
                WeakReference.SetTarget(converter = new(string.Empty));
            }

            return converter;
        }
    }

    /// <summary>
    /// Gets or sets identifier.
    /// </summary>
    public int Identifier { get; set; }

    /// <summary>
    /// Gets or sets value length.
    /// </summary>
    public int ValueLength { get; set; }

    /// <summary>
    /// Gets or sets value.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Convert instance to Extra Value.
    /// </summary>
    /// <returns>Extra Value.</returns>
    public CsfValueExtra ToExtra() => new(Identifier, ValueLength, Value, 0, string.Empty);

    /// <summary>
    /// Convert instance to Extra Value.
    /// </summary>
    /// <param name="extra">Extra Value String.</param>
    /// <returns>Extra Value.</returns>
    public CsfValueExtra ToExtra(string extra) => new(Identifier, ValueLength, Value, extra.Length, extra);
}
