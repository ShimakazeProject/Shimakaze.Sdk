namespace Shimakaze.Sdk.Ini.Parser;

/// <summary>
/// Ini Token
/// </summary>
/// <param name="Type"><see cref="IniTokenType"/></param>
/// <param name="Value"></param>
public record struct IniToken(int Type, string? Value = default)
{
    /// <inheritdoc cref="IniToken(int, string?)"/>
    /// <param name="type"><see cref="IniTokenType"/></param>
    /// <param name="value"></param>
    /// <param name="trim"></param>
    public IniToken(int type, string? value, bool trim)
        : this(type, value)
    {
        if (trim)
            Value = value?.Trim();
    }
}