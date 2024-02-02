using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml.Converter.V1;

/// <summary>
/// Csf Label Converter.
/// </summary>
public class CsfDataConverter : IYamlTypeConverter
{
    private static readonly WeakReference<CsfDataConverter> WeakReference = new(new());

    /// <summary>
    /// Gets csfValueConverter.
    /// </summary>
    public static CsfDataConverter Instance
    {
        get
        {
            if (!WeakReference.TryGetTarget(out CsfDataConverter? converter))
            {
                WeakReference.SetTarget(converter = new());
            }

            return converter;
        }
    }

    /// <inheritdoc />
    public bool Accepts(Type type) => typeof(CsfData).IsAssignableFrom(type);

    /// <inheritdoc />
    public object? ReadYaml(IParser parser, Type type)
    {
        // 检查是不是 CSF 标签
        if (!parser.TryConsume<Scalar>(out var label))
            throw new FormatException($"Unknown Format at {parser.Current?.Start} - {parser.Current?.End}");

        CsfData data = new(label.Value);
        List<CsfValue> values = [];
        if (parser.TryConsume<SequenceStart>(out _))
        {
            while (!parser.TryConsume<SequenceEnd>(out _))
            {
                ParseValue(parser, values);
            }
        }
        else
        {
            ParseValue(parser, values);
        }

        data.Values = [.. values];
        data.ReCount();
        return data;
    }

    /// <inheritdoc />
    public void WriteYaml(IEmitter emitter, object? value, Type type)
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
                CsfValueConverter.Instance.WriteYaml(emitter, item, item.GetType());
            }

            emitter.Emit(new SequenceEnd());
        }
        else
        {
            CsfValue v = data.Values.Length is > 0 ? data.Values.First() : CsfValue.Empty;
            CsfValueConverter.Instance.WriteYaml(emitter, v, v.GetType());
        }
    }

    private static void ParseValue(IParser parser, List<CsfValue> data)
    {
        if (CsfValueConverter.Instance.ReadYaml(parser, typeof(CsfValue)) is CsfValue value)
        {
            data.Add(value);
        }
    }
}