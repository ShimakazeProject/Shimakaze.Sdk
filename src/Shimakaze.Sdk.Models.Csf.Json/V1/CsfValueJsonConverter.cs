namespace Shimakaze.Sdk.Models.Csf.Json.V1;

public class CsfValueJsonConverter : JsonConverter<ICsfValue>
{
    public override ICsfValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        (string value, string extra) = (string.Empty, string.Empty);
        JsonConverter<string> converter = options.GetConverter<string>();
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
            case JsonTokenType.StartArray:
                value = converter.Read(ref reader, options)!;
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
                            value = converter.Read(ref reader, options)!;
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
        return string.IsNullOrEmpty(extra) ? ICsfValue.Create(value) : ICsfValue.Create(value, extra);
    }

    public override void Write(Utf8JsonWriter writer, ICsfValue value, JsonSerializerOptions options)
    {
        options.GetConverter<string>().Write(writer, value.Value, options);

        if (value is ICsfValueExtra extra)
        {
            writer.WriteString("extra", extra.ExtraValue);
        }
    }
}