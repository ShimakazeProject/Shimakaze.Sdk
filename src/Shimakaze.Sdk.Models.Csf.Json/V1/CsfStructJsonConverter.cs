namespace Shimakaze.Sdk.Models.Csf.Json.V1;

public class CsfStructJsonConverter : JsonConverter<ICsf>
{
    public override ICsf Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        ICsf result = ICsf.Create();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            switch (reader.GetString()?.ToLower() ?? throw new JsonException())
            {
                case "$schema":
                    reader.Skip();
                    break;

                case "protocol":
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.Number)
                    {
                        throw new JsonException();
                    }

                    if (reader.GetInt32() != 1)
                    {
                        throw new NotSupportedException("Supported protocol Version is 1 but it is " + reader.GetInt32());
                    }

                    break;

                case "head":
                    reader.Read();
                    result.Metadata = options.GetConverter<ICsfMetadata>().Read(ref reader, options)!;
                    break;

                case "data":
                    reader.Read();
                    result.Data = options.GetConverter<IList<ICsfData>>().Read(ref reader, options)!;
                    break;

                default:
                    break;
            }
        }
        result.ReCount();
        return result;
    }

    public override void Write(Utf8JsonWriter writer, ICsf value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("$schema", JsonConstants.SchemaUrls.V1);
        writer.WriteNumber("protocol", 1);
        writer.WritePropertyName("head");
        options.GetConverter<ICsfMetadata>().Write(writer, value.Metadata, options);
        writer.WritePropertyName("data");
        options.GetConverter<IList<ICsfData>>().Write(writer, value.Data, options);
        writer.WriteEndObject();
    }
}