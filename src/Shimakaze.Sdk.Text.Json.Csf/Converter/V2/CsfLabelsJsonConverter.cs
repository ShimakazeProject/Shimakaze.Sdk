using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Text.Json.Csf.Converter.V2;

/// <summary>
/// CsfLabelsJsonConverter.
/// </summary>
public class CsfLabelsJsonConverter : JsonConverter<IList<CsfData>>
{
    /// <inheritdoc/>
    public override IList<CsfData> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        JsonConverter<CsfData> converter = options.GetConverter<CsfData>();
        List<CsfData> result = new();
        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
            {
                break;
            }

            result.Add(converter.Read(ref reader, options)!);
        }

        return result.ToArray();
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IList<CsfData> value, JsonSerializerOptions options)
    {
        JsonConverter<CsfData> converter = options.GetConverter<CsfData>();
        writer.WriteStartObject();
        foreach (CsfData item in value)
        {
            converter.Write(writer, item, options);
        }

        writer.WriteEndObject();
    }
}
