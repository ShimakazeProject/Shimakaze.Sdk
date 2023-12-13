using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Csf;

/// <summary>
/// Csf Value.
/// </summary>
public record struct CsfValue
{
    internal int InternalIdentifier;
    internal int InternalValueLength;
    internal string InternalValue;
    internal int? InternalExtraValueLength;
    internal string? InternalExtraValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfValue" /> class.
    /// </summary>
    public CsfValue()
      : this(string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfValue" /> class.
    /// </summary>
    /// <param name="value"> value. </param>
    public CsfValue(string value)
    {
        InternalIdentifier = CsfConstants.StrFlagRaw;
        InternalValueLength = value.Length;
        InternalValue = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsfValue" /> class.
    /// </summary>
    /// <param name="value"> value. </param>
    /// <param name="extraValue"> extraValue. </param>
    public CsfValue(string value, string? extraValue)
      : this(value)
    {
        if (extraValue is null)
            return;

        Identifier = CsfConstants.StrwFlgRaw;
        ExtraValueLength = extraValue.Length;
        ExtraValue = extraValue;
    }

    /// <summary>
    /// Gets Empty.
    /// </summary>
    public static CsfValue Empty { get; } = new(string.Empty);

    /// <summary>
    /// HasExtra
    /// </summary>
    [MemberNotNullWhen(true, nameof(ExtraValueLength), nameof(ExtraValue))]
    public bool HasExtra
    {
        readonly get => InternalIdentifier is CsfConstants.StrwFlgRaw;
        set => Identifier = value ? CsfConstants.StrwFlgRaw : CsfConstants.StrFlagRaw;
    }

    /// <summary>
    /// Gets or sets identifier.
    /// </summary>
    public int Identifier
    {
        readonly get => InternalIdentifier;
        set => InternalIdentifier = value;
    }

    /// <summary>
    /// Gets or sets value length.
    /// </summary>
    public int ValueLength
    {
        readonly get => InternalValueLength;
        set => InternalValueLength = value;
    }

    /// <summary>
    /// Gets or sets value.
    /// </summary>
    public string Value
    {
        readonly get => InternalValue;
        set => InternalValue = value;
    }

    /// <summary>
    /// Gets or sets extra value length.
    /// </summary>
    public int? ExtraValueLength
    {
        readonly get => InternalExtraValueLength;
        set => InternalExtraValueLength = value;
    }

    /// <summary>
    /// Gets or sets extra value.
    /// </summary>
    public string? ExtraValue
    {
        readonly get => InternalExtraValue;
        set => InternalExtraValue = value;
    }
}