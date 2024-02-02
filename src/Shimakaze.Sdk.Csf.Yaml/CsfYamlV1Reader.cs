using Shimakaze.Sdk.Csf.Yaml.Converter.V1;

using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml;

/// <summary>
/// CSF YAML Deserializer.
/// </summary>
public static class CsfYamlV1Reader
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static CsfDocument Read(TextReader reader, Func<DeserializerBuilder, DeserializerBuilder>? builder = default)
    {
        builder ??= i => i;

        return builder(new())
            .WithTypeConverter(CsfValueConverter.Instance)
            .WithTypeConverter(CsfDataConverter.Instance)
            .WithTypeConverter(CsfDocumentConverter.Instance)
            .Build()
            .Deserialize<CsfDocument>(reader);
    }
}