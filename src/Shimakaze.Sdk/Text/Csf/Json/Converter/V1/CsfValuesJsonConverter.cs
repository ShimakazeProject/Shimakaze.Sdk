using System.Text.Json;
using System.Text.Json.Serialization;

using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Text.Csf.Json.Converter.V1;

/// <summary>
/// CsfValuesJsonConverter.
/// </summary>
public class CsfValuesJsonConverter : JsonConverter<IList<CsfValue>>
{
    /// <inheritdoc/>
    public override IList<CsfValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }

        JsonConverter<CsfValue> converter = options.GetConverter<CsfValue>();
        List<CsfValue> result = new();
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

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IList<CsfValue> value, JsonSerializerOptions options)
    {
        JsonConverter<CsfValue> converter = options.GetConverter<CsfValue>();
        if (value.Count > 1)
        {
            writer.WritePropertyName("values");
            writer.WriteStartArray();
            foreach (CsfValue item in value)
            {
                converter.Write(writer, item, options);
            }

            writer.WriteEndArray();
        }
        else if (value.Count == 1)
        {
            writer.WritePropertyName("value");
            converter.Write(writer, value[0], options);
        }
        else
        {
            writer.WriteNull("value");
        }
    }
}
