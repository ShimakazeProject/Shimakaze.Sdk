using System.Text.Json;
using System.Text.Json.Serialization;

using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Text.Csf.Json.Converter.V2;

/// <summary>
/// CsfLabelJsonConverter.
/// </summary>
public class CsfLabelJsonConverter : JsonConverter<CsfData>
{
    /// <inheritdoc/>
    public override CsfData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException();
        }

        CsfData result = new(reader.GetString()!);

        (string value, string extra) = (string.Empty, string.Empty);
        JsonConverter<IList<CsfValue>> converter = options.GetConverter<IList<CsfValue>>();
        JsonConverter<string> converter2 = options.GetConverter<string>();
        bool skipValue = false;

        while (reader.Read())
        {
        OUTER:
            if (reader.TokenType is JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType is JsonTokenType.StartArray or JsonTokenType.String)
            {
                value = converter2.Read(ref reader, options);
                break;
            }
            else if (reader.TokenType is JsonTokenType.StartObject)
            {
                while (reader.Read())
                {
                    if (reader.TokenType is JsonTokenType.EndObject)
                    {
                        goto OUTER;
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    if ((reader.GetString()?.ToLower() ?? throw new JsonException()) == "values")
                    {
                        reader.Read();
                        result.Values = converter.Read(ref reader, options);
                        break;
                    }

                    switch (reader.GetString()?.ToLower() ?? throw new JsonException())
                    {
                        case "value":
                            if (result.Values.Count > 0)
                            {
                                throw new JsonException();
                            }

                            reader.Read();
                            value = converter2.Read(ref reader, options);
                            continue;
                        case "extra":
                            if (result.Values.Count > 0)
                            {
                                throw new JsonException();
                            }

                            reader.Read();
                            extra = (reader.TokenType is JsonTokenType.String ? reader.GetString() : null) ?? throw new JsonException();
                            continue;
                        default:
                            throw new JsonException();
                    }
                }

                continue;
            }
            else if (reader.TokenType is JsonTokenType.Null)
            {
                result.Values = Array.Empty<CsfValue>();
                reader.Read();
                skipValue = true;
            }
            else
            {
                throw new JsonException();
            }
        }

        if (result.Values.Count < 1 && !skipValue)
        {
            result.Values = new[] { string.IsNullOrEmpty(extra) ? new CsfValue(value) : new CsfValueExtra(value, extra) };
        }

        return result;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CsfData value, JsonSerializerOptions options)
    {
        JsonConverter<IList<CsfValue>> converter = options.GetConverter<IList<CsfValue>>();
        writer.WritePropertyName(value.LabelName);
        if (value.Values.Count == 1)
        {
            if (value.Values[0] is not CsfValueExtra)
            {
                options.GetConverter<CsfValue>().Write(writer, value.Values[0], options);
                return;
            }
        }

        if (value.Values.Count == 0)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        converter.Write(writer, value.Values, options);
        writer.WriteEndObject();
    }
}
