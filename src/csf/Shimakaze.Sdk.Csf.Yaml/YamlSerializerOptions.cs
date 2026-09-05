using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml;

/// <summary>
/// 
/// </summary>
public sealed class YamlSerializerOptions
{
    /// <summary>
    /// 
    /// </summary>
    public IList<YamlConverter> Converters { get; } =
    [
        new CsfDataYamlConverterV1(),
        new CsfValueYamlConverterV1(),
    ];

    /// <summary>
    /// 
    /// </summary>
    public StaticContext StaticContext { get; set; } = new CsfYamlV1StaticContext();

    internal ISerializer Serializer
    {
        get
        {
            if (field is not null)
                return field;

            StaticSerializerBuilder builder = new(StaticContext);
            foreach (var item in Converters)
                builder.WithTypeConverter(item);

            return field = builder.Build();
        }
    }

    internal IDeserializer Deserializer
    {
        get
        {
            if (field is not null)
                return field;

            StaticDeserializerBuilder builder = new(StaticContext);
            foreach (var item in Converters)
                builder.WithTypeConverter(item);

            return field = builder.Build();
        }
    }
}
