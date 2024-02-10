namespace Shimakaze.Sdk.LanguageServer.Services.Ini.Parser.TokenProviders;

/// <summary>
/// INI节解析器
/// </summary>
[IniSymbolProvider(Token)]
public sealed class IniSectionTokenProvider : IIniParseProvider
{
    /// <inheritdoc cref="IniTokens.Section"/>
    public const int Token = IniTokens.Section;

    /// <inheritdoc/>
    public bool CanExecute(in ReadOnlySpan<char> line)
    {
        return line.Contains('[') && line.Contains(']');
    }

    /// <inheritdoc/>
    public IniSymbol Execute(in ReadOnlySpan<char> line, in int lineNum)
    {
        int start = line.IndexOf('[');
        int length = line[start..].IndexOf(']') ;

        return new(Token, lineNum, start, length, line.Slice(start, length));
    }
}
