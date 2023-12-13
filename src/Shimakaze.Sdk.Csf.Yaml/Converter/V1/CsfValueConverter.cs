using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml.Converter.V1;

/// <summary>
/// Csf Value Converter.
/// </summary>
public class CsfValueConverter : IYamlTypeConverter
{
    private static readonly WeakReference<CsfValueConverter> WeakReference = new(new());

    /// <summary>
    /// Gets csfValueConverter.
    /// </summary>
    public static CsfValueConverter Instance
    {
        get
        {
            if (!WeakReference.TryGetTarget(out CsfValueConverter? converter))
            {
                WeakReference.SetTarget(converter = new());
            }

            return converter;
        }
    }

    /// <inheritdoc />
    public bool Accepts(Type type) => typeof(CsfValue).IsAssignableFrom(type);

    /// <inheritdoc />
    public object? ReadYaml(IParser parser, Type type)
    {
        if (parser.TryConsume<Scalar>(out var scalar))
        {
            return new CsfValue(scalar.Value);
        }
        else if (parser.TryConsume<MappingStart>(out _))
        {
            Dictionary<string, string> map = [];
            string? key = null;
            while (!parser.TryConsume<MappingEnd>(out _))
            {
                if (parser.TryConsume<Scalar>(out var scalar2))
                {
                    if (key is null)
                    {
                        key = scalar2.Value;
                    }
                    else
                    {
                        map[key] = scalar2.Value;
                        key = null;
                    }
                }
            }

            map.TryGetValue("extra", out string? extra);
            return new CsfValue(map["value"], extra);
        }

        if (parser.Current is null)
        {
            throw new FormatException("???");
        }

        Mark start = parser.Current.Start;
        Mark end = parser.Current.End;
        throw new FormatException("Not Supported", new YamlException(start, end, "Unknown Token"));
    }

    /// <inheritdoc />
    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        switch (value)
        {
            case CsfValue extra when extra.HasExtra:
                emitter.Emit(new MappingStart());
                emitter.Emit(new Scalar("value"));
                emitter.Emit(new Scalar(extra.Value));
                emitter.Emit(new Scalar("extra"));
                emitter.Emit(new Scalar(extra.ExtraValue));
                emitter.Emit(new MappingEnd());
                break;

            case CsfValue csfValue:
                emitter.Emit(new Scalar(csfValue.Value));
                break;
        }
    }
}