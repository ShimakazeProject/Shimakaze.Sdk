namespace Shimakaze.Sdk.LanguageServer.Services.Ini.Parser.TokenProviders.Ares;

/// <summary>
/// 继承节语法糖解析器
/// </summary>
[IniSymbolProvider(Token, 1)]
public sealed class AresIniBaseSectionProvider : IIniParseProvider
{
    /// <inheritdoc cref="IniTokens.Section"/>
    public const int Token = IniTokens.Section;

    /// <inheritdoc/>
    public bool CanExecute(in ReadOnlySpan<char> line)
    {
        return line.Contains(':') && line.Contains('[') && line.Contains(']');
    }

    /// <inheritdoc/>
    public IniSymbol Execute(in ReadOnlySpan<char> line, in int lineNum)
    {
        int start = line.IndexOf(':');
        int offset = line[start..].IndexOf('[');
        int length = line[offset..].IndexOf(']');

        return new(Token, lineNum, start, length, line.Slice(start, length));
    }
}
