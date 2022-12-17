using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Text.Json.Csf.Converter.V1;

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
                    result.Metadata = options.GetConverter<CsfMetadata>().Read(ref reader, options);
                    break;

                case "data":
                    reader.Read();
                    result.Data = options.GetConverter<IList<CsfData>>().Read(ref reader, options);
                    break;

                default:
                    break;
            }
        }

        result.ReCount();
        return result;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CsfDocument value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("$schema", JsonConstants.SchemaUrls.V1);
        writer.WriteNumber("protocol", 1);
        writer.WritePropertyName("head");
        options.GetConverter<CsfMetadata>().Write(writer, value.Metadata, options);
        writer.WritePropertyName("data");
        options.GetConverter<IList<CsfData>>().Write(writer, value.Data, options);
        writer.WriteEndObject();
    }
}
