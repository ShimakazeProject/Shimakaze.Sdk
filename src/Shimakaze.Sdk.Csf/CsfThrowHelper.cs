namespace Shimakaze.Sdk.Csf;

/// <summary>
/// CsfThrowHelper.
/// </summary>
internal static class CsfThrowHelper
{
    /// <summary>
    /// IsCsfFile.
    /// </summary>
    /// <param name="flag">flag.</param>
    /// <returns>flag value.</returns>
    /// <exception cref="FormatException">Format is not true.</exception>
    public static int IsCsfFile(int flag)
        => flag is CsfConstants.CsfFlagRaw
            ? flag
            : throw new FormatException("It's not CSF File Flag.");

    /// <summary>
    /// IsLabel.
    /// </summary>
    /// <param name="flag">flag.</param>
    /// <param name="args">args.</param>
    /// <returns>flag value.</returns>
    /// <exception cref="FormatException">Format is not true.</exception>
    public static int IsLabel(int flag, Func<object[]> args)
        => flag is CsfConstants.LblFlagRaw
            ? flag
            : throw new FormatException(string.Format("It's not CSF Label Flag. #{0} at 0x{1:X8}.", args()));

    /// <summary>
    /// IsStringOrExtraString.
    /// </summary>
    /// <param name="flag">flag.</param>
    /// <param name="args">args.</param>
    /// <returns>flag value.</returns>
    /// <exception cref="FormatException">Format is not true.</exception>
    public static int IsStringOrExtraString(int flag, Func<object[]> args)
        => flag is CsfConstants.StrFlagRaw or CsfConstants.StrwFlgRaw
            ? flag
            : throw new FormatException(string.Format("It's not CSF String Flag #{0}:{1} at 0x{2:X8}.", args()));
}
