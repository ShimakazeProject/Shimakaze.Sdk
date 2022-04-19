namespace Shimakaze.Sdk.Models.Csf.Json.V1;

public class CsfLabelsJsonConverter : JsonConverter<IList<ICsfData>>
{
    public override IList<ICsfData> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        JsonConverter<ICsfData> converter = options.GetConverter<ICsfData>();
        List<ICsfData> result = new();
        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndArray)
            {
                break;
            }

            result.Add(converter.Read(ref reader, options)!);
        }
        return result.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, IList<ICsfData> value, JsonSerializerOptions options)
    {
        JsonConverter<ICsfData> converter = options.GetConverter<ICsfData>();
        writer.WriteStartArray();
        value.Each(i => converter.Write(writer, i, options));
        writer.WriteEndArray();
    }
}