using System.Text.Json;
using System.Text.Json.Serialization;

using Shimakaze.Sdk.Csf.Json.Converter.V1;

namespace Shimakaze.Sdk.Csf.Json.Converter.V2;

/// <summary>
/// Csf值 转换器
/// </summary>
public sealed class CsfDataValueJsonConverter : JsonConverter<IList<CsfValue>>
{
    /// <inheritdoc />
    public override IList<CsfValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String or JsonTokenType.StartArray => [
                new CsfValue(
                    reader
                        .GetNotNull<CsfSimpleValueJsonConverter, string>(options)
                )
            ],
            JsonTokenType.StartObject => ReadAdvancedValue(ref reader, options),
            _ => [],
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IList<CsfValue> value, JsonSerializerOptions options)
    {
        if (value.Count is 0)
        {
            writer.WriteNullValue();
        }
        else if (value.Count is not 1)
        {
            writer.WriteStartObject();
            writer.WriteStartArray("values");
            foreach (var item in value)
                writer.WriteValue<CsfValueJsonConverter, CsfValue>(item, options);

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        else if (value[0].ExtraValue is null)
        {
            writer.WriteValue<CsfSimpleValueJsonConverter, string>(value[0].Value, options);
        }
        else
        {
            writer.WriteStartObject();
            writer.WriteProperty<CsfSimpleValueJsonConverter, string>("value", value[0].Value, options);
            writer.WriteString("extra", value[0].ExtraValue);
            writer.WriteEndObject();
        }
    }

    private static List<CsfValue> ReadAdvancedValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        CsfJsonAsserts.IsToken(JsonTokenType.StartObject, reader.TokenType);

        CsfValue? value = null;
        List<CsfValue>? values = null;
        string? extra = null;

        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
                break;
            switch (reader.GetString()?.ToLowerInvariant())
            {
                case "value":
                    CsfJsonAsserts.PropertyIsNull(values, "value");
                    value = CsfDataJsonConverter.ReadValue(ref reader, options);
                    break;

                case "extra":
                    CsfJsonAsserts.PropertyIsNull(values, "extra");
                    extra = reader.ReadString();
                    break;

                case "values":
                    CsfJsonAsserts.PropertyIsNull(value);
                    values = CsfDataJsonConverter.ReadValues(ref reader, options);
                    break;

                default:
                    reader.Read();
                    break;
            }
        }

        if (values is not null)
            return values;

        CsfJsonAsserts.PropertyIsNotNull(value);

        return
        [
            string.IsNullOrEmpty(extra) switch
            {
                true => value.Value,
                false => new CsfValue(value.Value.Value, extra)
            }
        ];
    }
}