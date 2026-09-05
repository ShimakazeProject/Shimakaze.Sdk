namespace Shimakaze.Sdk.Csf.Yaml;

/// <summary>
/// YAML Serializer.
/// </summary>
public static class YamlSerializer
{
    /// <summary>
    /// Deserialize from text reader.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="reader"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static T Deserialize<T>(TextReader reader, YamlSerializerOptions? options = default)
    {
        options ??= new();
        return options.Deserializer.Deserialize<T>(reader);
    }

    /// <summary>
    /// Serialize to text writer.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public static void Serialize<T>(TextWriter writer, T value, YamlSerializerOptions? options = default)
    {
        options ??= new();
        options.Serializer.Serialize(writer, value);
    }
}
