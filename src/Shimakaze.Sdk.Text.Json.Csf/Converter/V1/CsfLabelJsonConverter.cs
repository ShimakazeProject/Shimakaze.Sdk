using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Text.Json.Csf.Converter.V1;

/// <summary>
/// CsfLabelJsonConverter.
/// </summary>
public class CsfLabelJsonConverter : JsonConverter<CsfData>
{
    /// <inheritdoc/>
    public override CsfData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        CsfData result = new();
        (string? value, string? extra) = (string.Empty, string.Empty);
        JsonConverter<IList<CsfValue>> converter = options.GetConverter<IList<CsfValue>>();
        JsonConverter<string> converter2 = options.GetConverter<string>();
        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string label = reader.GetString() ?? throw new JsonException();

            switch (label.ToLower())
            {
                case "label":
                    reader.Read();
                    result.LabelName = label;
                    break;

                case "values":
                    reader.Read();
                    result.Values = converter.Read(ref reader, options);
                    break;

                case "value":
                    if (result.Values.Count > 0)
                    {
                        throw new JsonException();
                    }

                    reader.Read();

                    value = reader.TokenType is JsonTokenType.Null
                      ? null
                      : converter2.Read(ref reader, options);

                    break;

                case "extra":
                    if (result.Values.Count > 0)
                    {
                        throw new JsonException();
                    }

                    reader.Read();
                    extra = (reader.TokenType is JsonTokenType.String ? reader.GetString() : null) ?? throw new JsonException();
                    break;

                default:
                    throw new JsonException();
            }
        }

        if (value is null)
        {
            result.Values = Array.Empty<CsfValue>();
        }
        else if (result.Values.Count < 1)
        {
            result.Values = new[] { extra is null ? new CsfValue(value) : new CsfValueExtra(value, extra) };
        }

        return result;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CsfData value, JsonSerializerOptions options)
    {
        JsonConverter<IList<CsfValue>> converter = options.GetConverter<IList<CsfValue>>();
        writer.WriteStartObject();
        writer.WriteString("label", value.LabelName);
        converter.Write(writer, value.Values, options);
        writer.WriteEndObject();
    }
}
