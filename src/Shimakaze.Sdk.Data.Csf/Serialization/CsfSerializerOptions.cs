namespace Shimakaze.Sdk.Data.Csf.Serialization;

/// <summary>
/// Csf Serializer Options.
/// </summary>
public class CsfSerializerOptions
{
    private static CsfSerializerOptions? @default = default;

    /// <summary>
    /// Gets default.
    /// </summary>
    public static CsfSerializerOptions Default => @default ??= new();
}
