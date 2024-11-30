using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml.Converter.V1;

/// <summary>
/// Csf Label Converter.
/// </summary>
public class CsfDataConverter : IYamlTypeConverter
{
    /// <inheritdoc />
    public bool Accepts(Type type)
    {
        return typeof(CsfData).IsAssignableFrom(type);
    }

    /// <inheritdoc />
    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        // 检查是不是 CSF 标签
        if (!parser.TryConsume<Scalar>(out Scalar? label))
        {
            throw new FormatException($"Unknown Format at {parser.Current?.Start} - {parser.Current?.End}");
        }

        CsfData data = new(label.Value);
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

        data.Values = [.. values];
        data.ReCount();
        return data;
    }

    /// <inheritdoc />
    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value is not CsfData data)
        {
            return;
        }

        emitter.Emit(new Scalar(data.LabelName));

        if (data.Values.Length > 1)
        {
            emitter.Emit(new SequenceStart(AnchorName.Empty, TagName.Empty, true, SequenceStyle.Block));
            foreach (CsfValue item in data.Values)
            {
                serializer(item, item.GetType());
            }

            emitter.Emit(new SequenceEnd());
        }
        else
        {
            CsfValue v = data.Values.Length is > 0 ? data.Values.First() : CsfValue.Empty;
            serializer(v, v.GetType());
        }
    }

}
