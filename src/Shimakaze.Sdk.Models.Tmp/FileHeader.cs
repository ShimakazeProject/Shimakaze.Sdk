namespace Shimakaze.Sdk.Models.Tmp;

/// <summary>
/// TMP File Header <br/>
/// <see href="https://modenc.renegadeprojects.com/TMP"/>
/// </summary>
public struct FileHeader
{
    /// <summary>
    /// size in blocks
    /// </summary>
    public uint BlockWidth;
    /// <summary>
    /// size in blocks
    /// </summary>
    public uint BlockHeight;
    /// <summary>
    /// size of each block
    /// </summary>
    public uint BlockImageWidth;
    /// <summary>
    /// size of each block
    /// </summary>
    public uint BlockImageHeight;
}
