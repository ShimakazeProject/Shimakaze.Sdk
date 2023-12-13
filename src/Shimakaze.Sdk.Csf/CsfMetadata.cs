using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Csf;
/// <summary>
/// CsfMetadata.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
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
    public int Identifier;

    /// <summary>
    /// Version.
    /// </summary>
    public int Version;

    /// <summary>
    /// LabelCount.
    /// </summary>
    public int LabelCount;

    /// <summary>
    /// StringCount.
    /// </summary>
    public int StringCount;

    /// <summary>
    /// Unknown.
    /// </summary>
    public int Unknown;

    /// <summary>
    /// Language.
    /// </summary>
    public int Language;
};