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
        int end = line.IndexOf('=');
        if (end is -1)
            return false;

        int comment = line.IndexOf(';');
        if (comment is -1)
            comment = line.Length + 1;

        return end < comment;
    }

    /// <inheritdoc/>
    public IniSymbol Execute(in ReadOnlySpan<char> line, in int lineNum)
    {
        int start = 0;
        int length = line.IndexOf('=');

        return new(Token, lineNum, start, length, line[..length]);
    }
}
