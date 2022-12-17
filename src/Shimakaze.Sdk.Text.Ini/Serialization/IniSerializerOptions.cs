namespace Shimakaze.Sdk.Text.Ini.Serialization;

/// <summary>
/// IniSerializerOptions.
/// </summary>
public sealed class IniSerializerOptions
{
    private static IniSerializerOptions? @default = default;

    /// <summary>
    /// Gets default.
    /// </summary>
    public static IniSerializerOptions Default => @default ??= new();

    /// <summary>
    /// Gets or sets a value indicating whether ignore Summary.
    /// </summary>
    public bool IgnoreSummary { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether wrapSigns.
    /// </summary>
    public bool WrapSigns { get; set; } = true;
}
