namespace Shimakaze.Sdk.Mix;

/// <summary>
/// Specifies the mode in which a <see cref="MixArchive"/> is opened.
/// </summary>
public enum MixArchiveMode
{
    /// <summary>
    /// Open an existing archive for reading only.
    /// </summary>
    Read,

    /// <summary>
    /// Create a new empty archive.
    /// </summary>
    Create,

    /// <summary>
    /// Open an existing archive for reading and modification.
    /// </summary>
    Update
}
