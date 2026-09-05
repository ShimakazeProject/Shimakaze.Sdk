using System.Globalization;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Shimakaze.Sdk.Csf.Yaml;

/// <summary>
/// Csf Value Yaml Converter.
/// </summary>
public sealed class CsfValueYamlConverterV1 : YamlConverter<CsfValue>
{
    /// <inheritdoc/>
    public override CsfValue? Read(IParser parser, ObjectDeserializer deserializer, YamlSerializerOptions options)
    {
        if (parser.TryConsume<Scalar>(out var scalar))
        {
            if (scalar.Value == "null")
                return null;

            return new(scalar.Value, null);
        }

        if (!parser.TryConsume<MappingStart>(out var start))
            throw new YamlException(parser.Current?.Start ?? default, parser.Current?.End ?? default, "Unknown CSF value format.");

        string? value = null;
        string? extra = null;

        while (!parser.TryConsume<MappingEnd>(out _))
        {
            var key = parser.Consume<Scalar>();

            switch (key.Value)
            {
                case "value":
                    value = parser.Consume<Scalar>().Value;
                    break;

                case "extra":
                    extra = parser.Consume<Scalar>().Value;
                    break;

                default:
                    parser.SkipThisAndNestedEvents();
                    break;
            }
        }

        return value is null
            ? throw new YamlException(start.Start, start.End, "CSF value does not contain value.")
            : new(value, extra);
    }

    /// <inheritdoc/>
    public override void Write(IEmitter emitter, CsfValue value, ObjectSerializer serializer, YamlSerializerOptions options)
    {
        if (value.Extra is not null)
        {
            emitter.Emit(new MappingStart());
            emitter.Emit(new Scalar("value"));
            WriteValue(emitter, value.Value);
            emitter.Emit(new Scalar("extra"));
            emitter.Emit(new Scalar(value.Extra));
            emitter.Emit(new MappingEnd());
            return;
        }

        WriteValue(emitter, value.Value);
    }

    private static void WriteValue(IEmitter emitter, string value)
    {
        if (value.Contains('\r') || value.Contains('\n'))
        {
            emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, value, ScalarStyle.Literal, true, true));
            return;
        }

        if (double.TryParse(value, CultureInfo.InvariantCulture, out _))
        {
            emitter.Emit(new Scalar(AnchorName.Empty, TagName.Empty, value, ScalarStyle.SingleQuoted, true, true));
            return;
        }

        emitter.Emit(new Scalar(value));
    }
}
