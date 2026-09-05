namespace Shimakaze.Sdk.Csf.Yaml;
/// <summary>
/// CSF YAML Deserializer.
/// </summary>
public static class CsfYamlV1Reader
{
    /// <summary>
    /// 从文本流读取 CSF 数据
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static CsfData Read(TextReader reader, YamlSerializerOptions? options = default)
        => YamlSerializer.Deserialize<CsfData>(reader, options);

    /// <summary>
    /// 从文本流读取 CSF 数据
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static CsfData Read(Stream stream, YamlSerializerOptions? options = default)
    {
        using StreamReader reader = new(stream, leaveOpen: true);
        return Read(reader, options);
    }
}
