using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Converter.V1;

/// <summary>
/// Csf简单值 转换器
/// </summary>
public sealed class CsfSimpleValueJsonConverter : JsonConverter<string>
{
    /// <inheritdoc />
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.StartArray => ReadStringArray(ref reader),
            JsonTokenType.String => reader.GetString()!,
            _ => reader.TokenType.ThrowNotSupportToken<string>()
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        // CR / LF / CRLF
        if (value.Contains('\n') || value.Contains('\r'))
        {
            using StringReader sr = new(value);

            writer.WriteStartArray();
            while (sr.Peek() >= 0)
                writer.WriteStringValue(sr.ReadLine());

            writer.WriteEndArray();
        }
        else
        {
            writer.WriteStringValue(value);
        }
    }

    private static string ReadStringArray(ref Utf8JsonReader reader)
    {
        StringBuilder sb = new();
        while (reader.Read().ThrowWhenNull())
        {
            if (reader.TokenType is JsonTokenType.EndArray)
                break;
            sb.AppendLine(reader.GetString());
        }

        // 去掉最后一个换行符的魔法
        sb.Length -= Environment.NewLine.Length;
        return sb.ToString();
    }
}