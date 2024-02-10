namespace Shimakaze.Sdk.LanguageServer.Services.Ini.Parser.TokenProviders;

/// <summary>
/// INI值解析器
/// </summary>
[IniSymbolProvider(Token)]
public sealed class IniValueTokenProvider : IIniParseProvider
{
    /// <inheritdoc cref="IniTokens.Value"/>
    public const int Token = IniTokens.Value;

    /// <inheritdoc/>
    public bool CanExecute(in ReadOnlySpan<char> line)
    {
        if (!line.Contains('='))
            return false;

        int comment = line.IndexOf(';');
        if (comment is -1)
            comment = line.Length + 1;
        int start = line.IndexOf('=');
        return start < comment;
    }

    /// <inheritdoc/>
    public IniSymbol Execute(in ReadOnlySpan<char> line, in int lineNum)
    {
        int start = line.IndexOf('=') + 1;
        int length = line[start..].IndexOf(';');
        if (length < 0)
            length = line.Length - start;

        return new(Token, lineNum, start, length, line.Slice(start, length));
    }
}
