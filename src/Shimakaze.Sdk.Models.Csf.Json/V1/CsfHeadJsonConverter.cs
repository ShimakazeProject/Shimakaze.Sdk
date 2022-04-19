namespace Shimakaze.Sdk.Models.Csf.Json.V1;

public class CsfHeadJsonConverter : JsonConverter<ICsfMetadata>
{
    public override ICsfMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        ICsfMetadata result = ICsfMetadata.Create();
        result.Identifier = Constants.CSF_FLAG_RAW;
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
                case "version":
                    reader.Read();
                    result.Version = reader.GetInt32();
                    break;

                case "language":
                    reader.Read();
                    if (reader.TokenType is JsonTokenType.Number)
                    {
                        result.Language = reader.GetInt32();
                    }
                    else if (reader.TokenType is JsonTokenType.String)
                    {
                        string code = reader.GetString() ?? throw new JsonException();
                        for (result.Language = 0; result.Language < JsonConstants.LanguageList.Length; result.Language++)
                        {
                            if (JsonConstants.LanguageList[result.Language].Equals(code))
                            {
                                break;
                            }
                        }
                    }
                    break;

                default:
                    throw new JsonException();
            }
        }
        return result;
    }

    public override void Write(Utf8JsonWriter writer, ICsfMetadata value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("version", value.Version);
        if (value.Language < JsonConstants.LanguageList.Length)
        {
            writer.WriteString("language", JsonConstants.LanguageList[value.Language]);
        }
        else
        {
            writer.WriteNumber("language", value.Language);
        }

        writer.WriteEndObject();
    }
}