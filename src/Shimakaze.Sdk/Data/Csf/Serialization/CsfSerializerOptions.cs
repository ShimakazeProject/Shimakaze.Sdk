namespace Shimakaze.Sdk.Data.Csf.Serialization;

/// <summary>
/// Csf Serializer Options.
/// </summary>
public class CsfSerializerOptions
{
    private static CsfSerializerOptions? s_default = default;

    /// <summary>
    /// Gets default.
    /// </summary>
    public static CsfSerializerOptions Default => s_default ??= new();
}
