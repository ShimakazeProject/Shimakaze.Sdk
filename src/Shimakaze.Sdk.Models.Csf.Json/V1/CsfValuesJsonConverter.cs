namespace Shimakaze.Sdk.Models.Csf.Json.V1;

public class CsfValuesJsonConverter : JsonConverter<IList<ICsfValue>>
{
    public override IList<ICsfValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        JsonConverter<ICsfValue> converter = options.GetConverter<ICsfValue>();
        List<ICsfValue> result = new();
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

    public override void Write(Utf8JsonWriter writer, IList<ICsfValue> value, JsonSerializerOptions options)
    {
        JsonConverter<ICsfValue> converter = options.GetConverter<ICsfValue>();
        if (value.Count > 1)
        {
            writer.WritePropertyName("values");
            writer.WriteStartArray();
            value.Each(i => converter.Write(writer, i, options));
            writer.WriteEndArray();
        }
        else if (value.Count == 1)
        {
            writer.WritePropertyName("value");
            converter.Write(writer, value[0], options);
        }
        else
        {
            Debug.Assert(false);
        }
    }
}