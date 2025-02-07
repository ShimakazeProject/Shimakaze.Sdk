using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml.Converter.V1;

/// <summary>
/// Csf Value Converter.
/// </summary>
public class CsfValueConverter : IYamlTypeConverter
{
    /// <inheritdoc />
    public bool Accepts(Type type)
    {
        return typeof(CsfValue).IsAssignableFrom(type);
    }

    /// <inheritdoc />
    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)

    {
        if (parser.TryConsume<Scalar>(out var scalar))
        {
            return new CsfValue(scalar.Value, null);
        }
        else if (parser.TryConsume<MappingStart>(out var start))
        {
            string? value = null;
            string? extra = null;
            MappingEnd? end;
            while (!parser.TryConsume<MappingEnd>(out end))
            {
                if (parser.TryConsume<Scalar>(out var property) && parser.TryConsume<Scalar>(out Scalar? propertyValue))
                {
                    if (property.Value is "value")
                    {
                        value = propertyValue.Value;
                    }
                    else if (property.Value is "extra")
                    {
                        extra = propertyValue.Value;
                    }
                }
            }

            return string.IsNullOrEmpty(value)
                ? throw new FormatException($"Cannot found Value at {start.Start} - {end?.End}")
                : (object)new CsfValue(value, extra);
        }

        throw new FormatException($"Unknown Format at {parser.Current?.Start} - {parser.Current?.End}");
    }

    /// <inheritdoc />
    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        switch (value)
        {
            case CsfValue extra when extra.Extra is not null:
                emitter.Emit(new MappingStart());
                emitter.Emit(new Scalar("value"));
                emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, extra.Value, ScalarStyle.Literal, true, true));
                emitter.Emit(new Scalar("extra"));
                emitter.Emit(new Scalar(extra.Extra));
                emitter.Emit(new MappingEnd());
                break;

            case CsfValue csfValue when csfValue.Value.Contains('\r') || csfValue.Value.Contains('\n'):
                emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, csfValue.Value, ScalarStyle.Literal, true, true));
                break;

            case CsfValue csfValue when double.TryParse(csfValue.Value, out _):
                emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, csfValue.Value, ScalarStyle.SingleQuoted, true, true));
                break;

            case CsfValue csfValue:
                emitter.Emit(new Scalar( csfValue.Value));
                break;
        }
    }
}
