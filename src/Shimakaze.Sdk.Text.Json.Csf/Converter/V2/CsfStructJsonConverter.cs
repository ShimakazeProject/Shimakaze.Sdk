using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Text.Json.Csf.Converter.V2;

/// <summary>
/// CsfStructJsonConverter.
/// </summary>
public class CsfStructJsonConverter : JsonConverter<CsfDocument>
{
    /// <inheritdoc/>
    public override CsfDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        CsfDocument result = new();
        CsfMetadata head = default;
        head.Version = 3;
        head.Language = 0;
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

                    if (reader.GetInt32() != 2)
                    {
                        throw new NotSupportedException("Supported protocol Version is 2 but it is " + reader.GetInt32());
                    }

                    break;

                case "version":
                    reader.Read();
                    head.Version = reader.GetInt32();
                    break;

                case "language":
                    reader.Read();
                    if (reader.TokenType is JsonTokenType.Number)
                    {
                        head.Language = reader.GetInt32();
                    }
                    else if (reader.TokenType is JsonTokenType.String)
                    {
                        string code = reader.GetString() ?? throw new JsonException();
                        for (head.Language = 0; head.Language < JsonConstants.LanguageList.Length; head.Language++)
                        {
                            if (JsonConstants.LanguageList[head.Language].Equals(code))
                            {
                                break;
                            }
                        }
                    }

                    break;
                case "data":
                    reader.Read();
                    result.Data = options.GetConverter<IList<CsfData>>().Read(ref reader, options);
                    break;

                default:
                    break;
            }
        }

        result.Metadata = head;
        result.ReCount();
        return result;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CsfDocument value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("$schema", JsonConstants.SchemaUrls.V2);
        writer.WriteNumber("protocol", 2);
        writer.WriteNumber("version", value.Metadata.Version);
        writer.WriteNumber("language", value.Metadata.Language);

        writer.WritePropertyName("data");
        options.GetConverter<IList<CsfData>>().Write(writer, value.Data, options);
        writer.WriteEndObject();
    }
}
