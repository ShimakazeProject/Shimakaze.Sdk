using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml.Converter.V1;

/// <summary>
/// Csf Label Converter.
/// </summary>
public class CsfLabelConverter : IYamlTypeConverter
{
    /// <inheritdoc />
    public bool Accepts(Type type)
    {
        return typeof(CsfLabel).IsAssignableFrom(type);
    }

    /// <inheritdoc />
    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        // 检查是不是 CSF 标签
        if (!parser.TryConsume<Scalar>(out var label))
        {
            throw new FormatException($"Unknown Format at {parser.Current?.Start} - {parser.Current?.End}");
        }

        List<CsfValue> values = [];
        if (parser.TryConsume<SequenceStart>(out _))
        {
            while (!parser.TryConsume<SequenceEnd>(out _))
            {
                if (rootDeserializer(typeof(CsfValue)) is CsfValue value)
                {
                    values.Add(value);
                }
            }
        }
        else
        {
            if (rootDeserializer(typeof(CsfValue)) is CsfValue value)
            {
                values.Add(value);
            }
        }

        CsfLabel data = new(label.Value, values);
        return data;
    }

    /// <inheritdoc />
    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value is not CsfLabel data)
        {
            return;
        }

        emitter.Emit(new Scalar(data.Name));

        if (data.Values.Count is 0)
        {
            emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, "null", ScalarStyle.ForcePlain, true, true));
        }
        else if (data.Values.Count is 1)
        {
            serializer(data.Values[0], typeof(CsfValue));
        }
        else
        {
            emitter.Emit(new SequenceStart(AnchorName.Empty, TagName.Empty, true, SequenceStyle.Block));
            foreach (CsfValue item in data.Values)
            {
                serializer(item, item.GetType());
            }

            emitter.Emit(new SequenceEnd());
        }
    }

}
