namespace Shimakaze.Sdk.Models.Csf.Json.V2;

public class CsfLabelJsonConverter : JsonConverter<ICsfData>
{
    public override ICsfData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException();
        }

        ICsfData result = ICsfData.Create(reader.GetString()!);

        (string value, string extra) = (string.Empty, string.Empty);
        JsonConverter<IList<ICsfValue>> converter = options.GetConverter<IList<ICsfValue>>();
        JsonConverter<string> converter2 = options.GetConverter<string>();

        while (reader.Read())
        {
        OUTER:
            if (reader.TokenType is JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType is JsonTokenType.StartArray or JsonTokenType.String)
            {
                value = converter2.Read(ref reader, options)!;
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

                    if ("values" == (reader.GetString()?.ToLower() ?? throw new JsonException()))
                    {
                        reader.Read();
                        result.Values = converter.Read(ref reader, options)!;
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
                            value = converter2.Read(ref reader, options)!;
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
            else
            {
                throw new JsonException();
            }
        }
        if (result.Values.Count < 1)
        {
            result.Values = new[] { string.IsNullOrEmpty(extra) ? ICsfValue.Create(value) : ICsfValue.Create(value, extra) };
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, ICsfData value, JsonSerializerOptions options)
    {
        JsonConverter<IList<ICsfValue>> converter = options.GetConverter<IList<ICsfValue>>();
        writer.WritePropertyName(value.LabelName);
        if (value.Values.Count == 1)
        {
            if (value.Values[0] is not ICsfValueExtra)
            {
                options.GetConverter<ICsfValue>().Write(writer, value.Values[0], options);
                return;
            }
        }
        writer.WriteStartObject();
        converter.Write(writer, value.Values, options);
        writer.WriteEndObject();
    }
}