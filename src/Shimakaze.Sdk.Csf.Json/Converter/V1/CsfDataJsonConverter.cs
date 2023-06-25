using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Converter.V1;

/// <summary>
/// Csf标签 转换器
/// </summary>
public sealed class CsfDataJsonConverter : JsonConverter<CsfData>
{
    /// <inheritdoc/>
    public override CsfData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.TokenType.ThrowWhenNotToken(JsonTokenType.StartObject);
        string label = string.Empty;
        CsfValue? value = null;
        List<CsfValue>? values = null;
        string? extra = null;

        while (reader.Read().ThrowWhenNull())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
                break;
            switch (reader.GetString()?.ToLowerInvariant())
            {
                case "label":
                    label = reader.ReadString().ThrowWhenNull();
                    break;

                case "value":
                    (values is null).ThrowWhenFalse("Unknown Type");
                    value = ReadValue(ref reader, options);
                    break;

                case "extra":
                    (values is null).ThrowWhenFalse("Unknown Type");
                    extra = reader.ReadString();
                    break;

                case "values":
                    (value is null).ThrowWhenFalse("Unknown Type");
                    values = ReadValues(ref reader, options);
                    break;

                default:
                    reader.Read().ThrowWhenNull();
                    break;
            }
        }

        if (values is not null)
        {
            return new(label)
            {
                Values = values.ToArray()
            };
        }
        else
        {
            value.ThrowWhenNull();

            return new(label)
            {
                Values = new CsfValue[] {string.IsNullOrEmpty(extra) switch
                    {
                        true => value.Value,
                        false => new CsfValue(value.Value.Value, extra)
                    }
                }
            };
        }
    }

    internal static CsfValue ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        reader.Read().ThrowWhenFalse();
        return reader.TokenType switch
        {
            JsonTokenType.String or JsonTokenType.StartArray => new(reader
                                .Get<CsfSimpleValueJsonConverter, string>(options)
                                .ThrowWhenNull()),
            JsonTokenType.StartObject => reader
                                .Get<CsfAdvancedValueJsonConverter, CsfValue>(options)
                                .ThrowWhenNull(),
            JsonTokenType.Null => new(),
            _ => reader.TokenType.ThrowNotSupportToken<CsfValue>(),
        };
    }

    internal static List<CsfValue> ReadValues(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        reader.Read().ThrowWhenFalse();
        reader.TokenType.ThrowWhenNotToken(JsonTokenType.StartArray);
        List<CsfValue> values = new();
        while (reader.Read().ThrowWhenNull())
        {
            if (reader.TokenType is JsonTokenType.EndArray)
                break;
            values.Add(
                reader
                    .Get<CsfValueJsonConverter, CsfValue>(options)
                    .ThrowWhenNull()
            );
        }
        return values;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CsfData value, JsonSerializerOptions options)
    {
        if (value.Values.Length is 1)
        {
            writer.WriteStartObject();
            writer.WriteString("label", value.LabelName);
            writer.WriteProperty<CsfSimpleValueJsonConverter, string>("value", value.Values[0].Value, options);
            if (value.Values[0].HasExtra)
                writer.WriteString("extra", value.Values[0].ExtraValue);

            writer.WriteEndObject();
        }
        else
        {
            writer.WriteStartObject();
            writer.WriteString("label", value.LabelName);
            writer.WriteStartArray("values");
            foreach (var item in value.Values)
                writer.WriteValue<CsfValueJsonConverter, CsfValue>(item, options);

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}