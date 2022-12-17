namespace Shimakaze.Sdk.Text.Json.Csf.Serialization;

/// <summary>
/// CsfJsonSerializerOptions.
/// </summary>
public sealed class CsfJsonSerializerOptions
{
    private static CsfJsonSerializerOptions? @default = default;

    /// <summary>
    /// Gets default.
    /// </summary>
    public static CsfJsonSerializerOptions Default => @default ??= new();

    /// <summary>
    /// Gets or sets a value indicating whether format.
    /// </summary>
    public bool WriteIndented { get; set; } = true;
}
