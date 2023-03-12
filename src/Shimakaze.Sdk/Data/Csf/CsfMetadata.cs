using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Data.Csf;
/// <summary>
/// CsfMetadata.
/// </summary>
/// <param name="Identifier">identifier.</param>
/// <param name="Version">version.</param>
/// <param name="LabelCount">labelCount.</param>
/// <param name="StringCount">stringCount.</param>
/// <param name="Unknown">unknown.</param>
/// <param name="Language">language.</param>
[StructLayout(LayoutKind.Explicit)]
public record struct CsfMetadata(
    [field: FieldOffset(sizeof(int) * 0)]
    int Identifier,
    [field: FieldOffset(sizeof(int) * 1)]
    int Version,
    [field: FieldOffset(sizeof(int) * 2)]
    int LabelCount,
    [field: FieldOffset(sizeof(int) * 3)]
    int StringCount,
    [field: FieldOffset(sizeof(int) * 4)]
    int Unknown,
    [field: FieldOffset(sizeof(int) * 5)]
    int Language
)
{
    /// <summary>
    /// CsfMetadata.
    /// </summary>
    /// <param name="version">version.</param>
    /// <param name="language">language.</param>
    public CsfMetadata(int version, int language) : this(CsfConstants.CsfFlagRaw, version, 0, 0, 0, language)
    {
    }
};