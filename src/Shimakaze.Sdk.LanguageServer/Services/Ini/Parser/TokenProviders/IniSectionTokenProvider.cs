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
        if (!line.Contains('[') || !line.Contains(']'))
            return false;

        int comment = line.IndexOf(';');
        if (comment is -1)
            comment = line.Length + 1;

        int start = line.IndexOf('[');
        int end = line.IndexOf(']');
        return end < comment && start < end;
    }

    /// <inheritdoc/>
    public IniSymbol Execute(in ReadOnlySpan<char> line, in int lineNum)
    {
        int start = line.IndexOf('[') ;
        int length = line[start..].IndexOf(']') + 1;

        return new(Token, lineNum, start, length, line.Slice(start, length));
    }
}
