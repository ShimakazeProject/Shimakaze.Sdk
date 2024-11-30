using Shimakaze.Sdk.Csf.Yaml.Converter.V1;

using YamlDotNet.Serialization;

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
    /// <param name="builder"></param>
    public static void Write(TextWriter writer, CsfDocument value, Func<SerializerBuilder, SerializerBuilder>? builder = null)
    {
        builder ??= i => i;

        builder(new())
            .WithTypeConverter(new CsfValueConverter())
            .WithTypeConverter(new CsfDataConverter())
            .WithTypeConverter(new CsfDocumentConverter())
            .Build()
            .Serialize(writer, value);
    }
}
