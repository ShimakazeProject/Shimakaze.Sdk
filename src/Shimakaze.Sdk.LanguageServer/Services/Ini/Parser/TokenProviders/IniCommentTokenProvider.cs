namespace Shimakaze.Sdk.LanguageServer.Services.Ini.Parser.TokenProviders;

/// <summary>
/// 注释解析器
/// </summary>
[IniSymbolProvider(Token, int.MaxValue)]
public sealed class IniCommentTokenProvider : IIniParseProvider
{
    /// <inheritdoc cref="IniTokens.Comment"/>
    public const int Token = IniTokens.Comment;

    /// <inheritdoc/>
    public bool CanExecute(in ReadOnlySpan<char> line)
    {
        return line.Contains(';');
    }

    /// <inheritdoc/>
    public IniSymbol Execute(in ReadOnlySpan<char> line, in int lineNum)
    {
        int start = line.IndexOf(';');

        return new(Token, lineNum, start, line.Length - start, line[start..]);
    }
}
