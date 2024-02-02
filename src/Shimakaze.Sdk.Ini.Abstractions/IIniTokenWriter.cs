namespace Shimakaze.Sdk.Ini;

/// <summary>
/// INI Token 流写入器
/// </summary>
public interface IIniTokenWriter : IDisposable
{
    /// <summary>
    /// 写入Token
    /// </summary>
    /// <param name="token"></param>
    void Write(in IIniToken token);

    /// <summary>
    /// 写入换行符
    /// </summary>
    void WriteLine();

    /// <summary>
    /// 写入Token 后写入换行符
    /// </summary>
    /// <param name="token"></param>
    void WriteLine(in IIniToken token);

    /// <summary>
    /// 冲写流
    /// </summary>
    void Flush();
}

/// <summary>
/// INI Token 流写入器
/// </summary>
/// <typeparam name="TIniDocument"></typeparam>
/// <typeparam name="TIniSection"></typeparam>
public interface IIniTokenWriter<TIniDocument, TIniSection> : IIniTokenWriter
    where TIniDocument : IIniDocument<TIniSection>
    where TIniSection : IIniSection
{
    /// <summary>
    /// 写INI文件
    /// </summary>
    /// <param name="document"></param>
    void Write(in TIniDocument document);

    /// <summary>
    /// 写入节
    /// </summary>
    /// <param name="section"></param>
    void Write(in TIniSection section);

    /// <summary>
    /// 写入键值对
    /// </summary>
    /// <param name="keyValuePair"></param>
    void Write(in KeyValuePair<string, string> keyValuePair);
}