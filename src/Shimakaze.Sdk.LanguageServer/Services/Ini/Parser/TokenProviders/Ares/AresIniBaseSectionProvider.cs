namespace Shimakaze.Sdk.LanguageServer.Services.Ini.Parser.TokenProviders.Ares;

/// <summary>
/// 继承节语法糖解析器
/// </summary>
[IniSymbolProvider(Token, 1)]
public sealed class AresIniBaseSectionProvider : IIniParseProvider
{
    /// <inheritdoc cref="IniTokens.BaseSection"/>
    public const int Token = IniTokens.BaseSection;

    /// <inheritdoc/>
    public bool CanExecute(in ReadOnlySpan<char> line)
    {
        if (!line.Contains(':'))
            return false;

        int comment = line.IndexOf(';');
        if (comment is -1)
            comment = line.Length + 1;

        int start = line.IndexOf(':');
        if (start > comment)
            return false;

        var span = line[start..];
        return span.Contains('[') && span.Contains(']');
    }

    /// <inheritdoc/>
    public IniSymbol Execute(in ReadOnlySpan<char> line, in int lineNum)
    {
        int start = line.IndexOf(':');
        int offset = line[start..].IndexOf('[');
        int length = line[offset..].IndexOf(']') + 1;

        return new(Token, lineNum, start, length, line.Slice(start, length));
    }
}
