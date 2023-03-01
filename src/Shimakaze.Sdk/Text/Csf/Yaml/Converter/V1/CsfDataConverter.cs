using Shimakaze.Sdk.Data.Csf;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Text.Csf.Yaml.Converter.V1;

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

    /// <inheritdoc/>
    public bool Accepts(Type type) => typeof(CsfData).IsAssignableFrom(type);

    /// <inheritdoc/>
    public object? ReadYaml(IParser parser, Type type)
    {
        if (parser.Accept<Scalar>(out var label))
        {
            CsfData data = new(label.Value);
            if (parser.TryConsume<Scalar>(out _))
            {
                ParseValue(parser, data);
            }
            else if (parser.TryConsume<SequenceStart>(out _))
            {
                while (!parser.TryConsume<SequenceEnd>(out _))
                {
                    ParseValue(parser, data);
                }
            }

            return data;
        }

        if (parser.Current is null)
        {
            throw new FormatException("???");
        }

        Mark start = parser.Current.Start;
        Mark end = parser.Current.End;
        throw new FormatException("Not Supported", new YamlException(start, end, $"Unknown Token {parser.Current}"));
    }

    /// <inheritdoc/>
    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value is not CsfData data)
        {
            return;
        }

        emitter.Emit(new Scalar(data.LabelName));

        if (data.Values.Count > 1)
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
            CsfValue v = data.Values.FirstOrDefault() ?? CsfValue.Empty;
            CsfValueConverter.Instance.WriteYaml(emitter, v, v.GetType());
        }
    }

    private static void ParseValue(IParser parser, CsfData data)
    {
        if (CsfValueConverter.Instance.ReadYaml(parser, typeof(CsfValue)) is CsfValue value)
        {
            data.Add(value);
        }
    }
}
