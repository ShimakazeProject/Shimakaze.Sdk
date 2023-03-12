using System.Text.Json;
using System.Text.Json.Serialization;

using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Text.Csf.Json.Converter.V2;

/// <summary>
/// Csf值 转换器
/// </summary>
public sealed class CsfDataValueJsonConverter : JsonConverter<IList<CsfValue>>
{
    /// <inheritdoc/>
    public override IList<CsfValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String or JsonTokenType.StartArray => new(){
                new CsfValue(
                    reader
                        .Get<V1.CsfSimpleValueJsonConverter, string>(options)
                        .ThrowWhenNull()
                )
            },
            JsonTokenType.StartObject => ReadAdvancedValue(ref reader, options),
            JsonTokenType.Null => new(),
            _ => reader.TokenType.ThrowNotSupportToken<List<CsfValue>>(),
        };
    }

    private static List<CsfValue> ReadAdvancedValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        reader.TokenType.ThrowWhenNotToken(JsonTokenType.StartObject);
        CsfValue? value = null;
        List<CsfValue>? values = null;
        string? extra = null;

        while (reader.Read().ThrowWhenNull())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
                break;
            switch (reader.GetString()?.ToLowerInvariant())
            {
                case "value":
                    (values is null).ThrowWhenFalse("Unknown Type");
                    value = V1.CsfDataJsonConverter.ReadValue(ref reader, options);
                    break;

                case "extra":
                    (values is null).ThrowWhenFalse("Unknown Type");
                    extra = reader.ReadString();
                    break;

                case "values":
                    (value is null).ThrowWhenFalse("Unknown Type");
                    values = V1.CsfDataJsonConverter.ReadValues(ref reader, options);
                    break;

                default:
                    reader.Read().ThrowWhenNull();
                    break;
            }
        }

        if (values is not null)
            return values;

        value.ThrowWhenNull();

        return new()
        {
            string.IsNullOrEmpty(extra) switch
            {
                true => value,
                false => new CsfValueExtra(value.Value, extra)
            }
        };
    }

    /// <inheritdoc/>
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
                writer.WriteValue<V1.CsfValueJsonConverter, CsfValue>(item, options);

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        else if (value[0] is not CsfValueExtra extra)
        {
            writer.WriteValue<V1.CsfSimpleValueJsonConverter, string>(value[0].Value, options);
        }
        else
        {
            writer.WriteStartObject();
            writer.WriteProperty<V1.CsfSimpleValueJsonConverter, string>("value", value[0].Value, options);
            writer.WriteString("extra", extra.ExtraValue);
            writer.WriteEndObject();
        }
    }
}
