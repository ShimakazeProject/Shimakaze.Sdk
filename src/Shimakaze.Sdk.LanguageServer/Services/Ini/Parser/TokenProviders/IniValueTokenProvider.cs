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
        return line.Contains('=');
    }

    /// <inheritdoc/>
    public IniSymbol Execute(in ReadOnlySpan<char> line, in int lineNum)
    {
        int start = line.IndexOf('=');
        int length = line[start..].IndexOf(';') - 1;

        return new(Token, lineNum, start, length, line[start..length]);
    }
}
