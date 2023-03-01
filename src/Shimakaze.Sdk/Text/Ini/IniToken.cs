namespace Shimakaze.Sdk.Text.Ini;

/// <summary>
/// INI Tokens.
/// </summary>
public static class IniToken
{
    /// <summary>
    /// Empty Line.
    /// </summary>
    public const int EmptyLine = 0;

    /// <summary>
    /// Section Header.
    /// </summary>
    public const int SectionHeader = 1;

    /// <summary>
    /// Key.
    /// </summary>
    public const int Key = 2;

    /// <summary>
    /// Value.
    /// </summary>
    public const int Value = 3;

    /// <summary>
    /// Comment.
    /// </summary>
    public const int Comment = 4;

    /// <summary>
    /// PreProcessor Command.
    /// </summary>
    public const int PreProcessorCommand = 5;
}
