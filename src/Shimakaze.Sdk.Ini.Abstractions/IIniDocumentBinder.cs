namespace Shimakaze.Sdk.Ini;

/// <summary>
/// IniDocument 绑定器
/// </summary>
public interface IIniDocumentBinder<TIniDocument, TIniSection>
    where TIniDocument : IIniDocument<TIniSection>
    where TIniSection : IIniSection
{
    /// <summary>
    /// 绑定到 IniDocument
    /// </summary>
    /// <param name="ini"></param>
    /// <returns></returns>
    TIniDocument Bind(TIniDocument? ini = default);
}