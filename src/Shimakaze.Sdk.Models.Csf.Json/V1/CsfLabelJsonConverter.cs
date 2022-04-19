namespace Shimakaze.Sdk.Models.Csf.Json.V1;

public class CsfLabelJsonConverter : JsonConverter<ICsfData>
{
    public override ICsfData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        ICsfData result = ICsfData.Create();
        (string value, string extra) = (string.Empty, string.Empty);
        JsonConverter<IList<ICsfValue>> converter = options.GetConverter<IList<ICsfValue>>();
        JsonConverter<string> converter2 = options.GetConverter<string>();
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
                case "label":
                    reader.Read();
                    result.LabelName = reader.GetString()!;
                    break;

                case "values":
                    reader.Read();
                    result.Values = converter.Read(ref reader, options)!;
                    break;

                case "value":
                    if (result.Values.Count > 0)
                    {
                        throw new JsonException();
                    }

                    reader.Read();

                    value = converter2.Read(ref reader, options)!;

                    break;

                case "extra":
                    if (result.Values.Count > 0)
                    {
                        throw new JsonException();
                    }

                    reader.Read();
                    extra = (reader.TokenType is JsonTokenType.String ? reader.GetString() : null) ?? throw new JsonException();
                    break;

                default:
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
        writer.WriteStartObject();
        writer.WriteString("label", value.LabelName);
        converter.Write(writer, value.Values, options);
        writer.WriteEndObject();
    }
}