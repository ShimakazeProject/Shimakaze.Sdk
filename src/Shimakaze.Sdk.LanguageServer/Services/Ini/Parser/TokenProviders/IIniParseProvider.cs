namespace Shimakaze.Sdk.LanguageServer.Services.Ini.Parser.TokenProviders;

/// <summary>
/// INI 解析 提供程序
/// </summary>
public interface IIniParseProvider
{
    /// <summary>
    /// 判断 <paramref name="line"/> 是否可以被解析
    /// </summary>
    /// <param name="line">当前行内容</param>
    /// <returns>
    /// <paramref name="line"/> 是否可以被解析<br/>
    /// <list type="bullet">
    ///     <item>
    ///         <see langword="true"/> 可以被解析
    ///     </item>
    ///     <item>
    ///         <see langword="false"/> 不可以被解析
    ///     </item>
    /// </list>
    /// </returns>
    bool CanExecute(in ReadOnlySpan<char> line);

    /// <summary>
    /// 解析 <paramref name="line"/> 
    /// </summary>
    /// <param name="line">当前行内容</param>
    /// <param name="lineNum"><paramref name="line"/> 所在的行号</param>
    /// <returns>解析后的结果</returns>
    IniSymbol Execute(in ReadOnlySpan<char> line, in int lineNum);
}
