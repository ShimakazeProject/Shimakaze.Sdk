namespace Shimakaze.Sdk.Csf.Yaml;

/// <summary>
/// CSF YAML Serializer.
/// </summary>
public static class CsfYamlV1Writer
{
    /// <summary>
    /// 写入到文本流
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public static void Write(TextWriter writer, CsfData value, YamlSerializerOptions? options = default)
        => YamlSerializer.Serialize(writer, value, options);

    /// <summary>
    /// 写入到文本流
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public static void Write(Stream stream, CsfData value, YamlSerializerOptions? options = default)
    {
        using StreamWriter writer = new(stream, leaveOpen: true);
        Write(writer, value, options);
    }
}
