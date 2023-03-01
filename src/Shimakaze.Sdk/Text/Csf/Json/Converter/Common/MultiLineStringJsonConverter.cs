
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Shimakaze.Sdk.Text.Csf.Json.Converter.Common;

/// <summary>
/// Multi Line String JsonConverter.
/// </summary>
public class MultiLineStringJsonConverter : JsonConverter<string?>
{
    /// <inheritdoc/>
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartArray:
                StringBuilder sb = new();
                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.EndArray:
                            if (sb is null)
                            {
                                throw new JsonException();
                            }

                            return sb.ToString().TrimEnd();

                        case JsonTokenType.String:
                            sb.AppendLine(reader.GetString());
                            break;

                        default:
                            throw new JsonException();
                    }
                }

                throw new JsonException();
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Null:
                return null;
            default:
                throw new JsonException();
        }
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        List<string> lines = value.Split('\n').Select(i => i.TrimEnd('\r')).ToList();
        if (lines.Count == 1)
        {
            writer.WriteStringValue(lines[0]);
        }
        else
        {
            writer.WriteStartArray();
            lines.ForEach(writer.WriteStringValue);
            writer.WriteEndArray();
        }
    }
}
