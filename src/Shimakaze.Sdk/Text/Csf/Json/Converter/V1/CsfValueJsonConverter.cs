using System.Text.Json;
using System.Text.Json.Serialization;

using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Text.Csf.Json.Converter.V1;

/// <summary>
/// CsfValueJsonConverter.
/// </summary>
public class CsfValueJsonConverter : JsonConverter<CsfValue>
{
    /// <inheritdoc/>
    public override CsfValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        (string value, string extra) = (string.Empty, string.Empty);
        JsonConverter<string> converter = options.GetConverter<string>();
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
            case JsonTokenType.StartArray:
                value = converter.Read(ref reader, options);
                break;

            case JsonTokenType.StartObject:
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

                    switch (reader.GetString()?.ToLower() ?? throw new JsonException())
                    {
                        case "value":
                            reader.Read();
                            value = converter.Read(ref reader, options);
                            break;

                        case "extra":
                            reader.Read();
                            extra = (reader.TokenType is JsonTokenType.String ? reader.GetString() : null) ?? throw new JsonException();
                            break;

                        default:
                            throw new JsonException();
                    }
                }

                break;

            default:
                throw new JsonException();
        }

        return string.IsNullOrEmpty(extra) ? new CsfValue(value) : new CsfValueExtra(value, extra);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CsfValue value, JsonSerializerOptions options)
    {
        options.GetConverter<string>().Write(writer, value.Value, options);

        if (value is CsfValueExtra extra)
        {
            writer.WriteString("extra", extra.ExtraValue);
        }
    }
}
