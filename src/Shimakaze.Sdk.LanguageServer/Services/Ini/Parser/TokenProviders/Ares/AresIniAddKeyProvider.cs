namespace Shimakaze.Sdk.LanguageServer.Services.Ini.Parser.TokenProviders.Ares;

/// <summary>
/// Ares += 语法糖解析器
/// </summary>
[IniSymbolProvider(Token, 1)]
public sealed class AresIniAddKeyProvider : IIniParseProvider
{
    /// <inheritdoc cref="IniTokens.AddKey"/>
    public const int Token = IniTokens.AddKey;

    /// <inheritdoc/>
    public bool CanExecute(in ReadOnlySpan<char> line)
    {
        int sign = line.IndexOf('+');
        if (sign is -1)
            return false;

        int comment = line.IndexOf(';');
        if (comment is -1)
            comment = line.Length + 1;

        if (sign > comment)
            return false;

        if (sign + 1 >= line.Length)
            return false;

        return line[sign + 1] is '=';
    }

    /// <inheritdoc/>
    public IniSymbol Execute(in ReadOnlySpan<char> line, in int lineNum)
    {
        int start = line.IndexOf('+');
        int length = 2;

        return new(Token, lineNum, start, length, line.Slice(start, length));
    }
}