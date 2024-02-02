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
        else if (parser.TryConsume<MappingStart>(out var start))
        {
            MappingEnd? end;
            string? value = null;
            string? extra = null;
            while (!parser.TryConsume<MappingEnd>(out end))
            {
                if (parser.TryConsume<Scalar>(out var property) && parser.TryConsume<Scalar>(out var propertyValue))
                {
                    if(property.Value is "value")
                    {
                        value = propertyValue.Value;
                    }
                    else if (property.Value is "extra")
                    {
                        extra = propertyValue.Value;
                    }
                }
            }

            if (string.IsNullOrEmpty(value))
                throw new FormatException($"Cannot found Value at {start.Start} - {end?.End}");

            return new CsfValue(value, extra);
        }

        throw new FormatException($"Unknown Format at {parser.Current?.Start} - {parser.Current?.End}");
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