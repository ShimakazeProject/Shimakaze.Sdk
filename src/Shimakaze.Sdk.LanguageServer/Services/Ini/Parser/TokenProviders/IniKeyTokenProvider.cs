namespace Shimakaze.Sdk.LanguageServer.Services.Ini.Parser.TokenProviders;

/// <summary>
/// INI键解析器
/// </summary>
[IniSymbolProvider(Token)]
public sealed class IniKeyTokenProvider : IIniParseProvider
{
    /// <inheritdoc cref="IniTokens.Key"/>
    public const int Token = IniTokens.Key;

    /// <inheritdoc/>
    public bool CanExecute(in ReadOnlySpan<char> line)
    {
        return line.Contains('=');
    }

    /// <inheritdoc/>
    public IniSymbol Execute(in ReadOnlySpan<char> line, in int lineNum)
    {
        int start = 0;
        int length = line.IndexOf('=');

        return new(Token, lineNum, start, length, line[..length]);
    }
}
