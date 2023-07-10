using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Csf;
/// <summary>
/// CsfMetadata.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public record struct CsfMetadata
{
    /// <summary>
    /// CsfMetadata.
    /// </summary>
    public CsfMetadata()
    {
    }

    /// <summary>
    /// CsfMetadata.
    /// </summary>
    /// <param name="version"> version. </param>
    /// <param name="language"> language. </param>
    public CsfMetadata(int version, int language)
    {
        Identifier = CsfConstants.CsfFlagRaw;
        Version = version;
        LabelCount = 0;
        StringCount = 0;
        Unknown = 0;
        Language = language;
    }

    /// <summary>
    /// Identifier.
    /// </summary>
    [FieldOffset(sizeof(int) * 0)]
    public int Identifier;

    /// <summary>
    /// Version.
    /// </summary>
    [FieldOffset(sizeof(int) * 1)]
    public int Version;

    /// <summary>
    /// LabelCount.
    /// </summary>
    [FieldOffset(sizeof(int) * 2)]
    public int LabelCount;

    /// <summary>
    /// StringCount.
    /// </summary>
    [FieldOffset(sizeof(int) * 3)]
    public int StringCount;

    /// <summary>
    /// Unknown.
    /// </summary>
    [FieldOffset(sizeof(int) * 4)]
    public int Unknown;

    /// <summary>
    /// Language.
    /// </summary>
    [FieldOffset(sizeof(int) * 5)]
    public int Language;
};