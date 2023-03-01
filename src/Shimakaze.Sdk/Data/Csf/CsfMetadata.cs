using System.Runtime.InteropServices;

namespace Shimakaze.Sdk.Data.Csf;
/// <summary>
/// CsfMetadata.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public record struct CsfMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsfMetadata"/> class.
    /// </summary>
    /// <param name="identifier">identifier.</param>
    /// <param name="version">version.</param>
    /// <param name="labelCount">labelCount.</param>
    /// <param name="stringCount">stringCount.</param>
    /// <param name="unknown">unknown.</param>
    /// <param name="language">language.</param>
    public CsfMetadata(int identifier, int version, int labelCount, int stringCount, int unknown, int language)
    {
        this.Identifier = identifier;
        this.Version = version;
        this.LabelCount = labelCount;
        this.StringCount = stringCount;
        this.Unknown = unknown;
        this.Language = language;
    }

    /// <summary>
    /// Gets or sets identifier.
    /// </summary>
    [field: FieldOffset(sizeof(int) * 0)]
    public int Identifier { readonly get; set; }

    /// <summary>
    /// Gets or sets version.
    /// </summary>
    [field: FieldOffset(sizeof(int) * 1)]
    public int Version { readonly get; set; }

    /// <summary>
    /// Gets or sets labelCount.
    /// </summary>
    [field: FieldOffset(sizeof(int) * 2)]
    public int LabelCount { readonly get; set; }

    /// <summary>
    /// Gets or sets stringCount.
    /// </summary>
    [field: FieldOffset(sizeof(int) * 3)]
    public int StringCount { readonly get; set; }

    /// <summary>
    /// Gets or sets unknown.
    /// </summary>
    [field: FieldOffset(sizeof(int) * 4)]
    public int Unknown { readonly get; set; }

    /// <summary>
    /// Gets or sets language.
    /// </summary>
    [field: FieldOffset(sizeof(int) * 5)]
    public int Language { readonly get; set; }
}
