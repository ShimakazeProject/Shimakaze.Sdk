using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Converter.V1;

/// <summary>
/// Csf标签 转换器
/// </summary>
public sealed class CsfDataJsonConverter : JsonConverter<CsfData>
{
    /// <inheritdoc />
    public override CsfData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        CsfJsonAsserts.IsToken(JsonTokenType.StartObject, reader.TokenType);
        string label = string.Empty;
        CsfValue? value = null;
        List<CsfValue>? values = null;
        string? extra = null;

        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
                break;
            switch (reader.GetString()?.ToLowerInvariant())
            {
                case "label":
                    label = reader.ReadString();
                    break;

                case "value":
                    CsfJsonAsserts.PropertyIsNull(values, nameof(value));
                    value = ReadValue(ref reader, options);
                    break;

                case "extra":
                    CsfJsonAsserts.PropertyIsNull(values, nameof(extra));
                    extra = reader.ReadStringOrNull();
                    break;

                case "values":
                    CsfJsonAsserts.PropertyIsNull(value);
                    values = ReadValues(ref reader, options);
                    break;

                default:
                    reader.Read();
                    break;
            }
        }

        if (values is not null)
        {
            return new(label)
            {
                Values = [.. values]
            };
        }
        else
        {
            CsfJsonAsserts.PropertyIsNotNull(value);

            return new(label)
            {
                Values = [string.IsNullOrEmpty(extra) switch
                {
                    true => value.Value,
                    false => new CsfValue(value.Value.Value, extra)
                }
                ]
            };
        }
    }

    /// <inheritdoc />
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

    internal static CsfValue ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        CsfJsonAsserts.IsNotEndOfStream(reader.Read());
        return reader.TokenType switch
        {
            JsonTokenType.String or JsonTokenType.StartArray =>
                new(reader.GetNotNull<CsfSimpleValueJsonConverter, string>(options)),
            JsonTokenType.StartObject =>
                reader.GetNotNull<CsfAdvancedValueJsonConverter, CsfValue>(options),
            _ => new()
        };

    }

    internal static List<CsfValue> ReadValues(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        CsfJsonAsserts.IsNotEndOfStream(reader.Read());
        CsfJsonAsserts.IsToken(JsonTokenType.StartArray, reader.TokenType);
        List<CsfValue> values = [];
        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndArray)
                break;
            values.Add(reader.GetNotNull<CsfValueJsonConverter, CsfValue>(options));
        }
        return values;
    }
}