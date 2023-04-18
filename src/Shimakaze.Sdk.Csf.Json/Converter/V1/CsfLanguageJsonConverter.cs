using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Converter.V1;

/// <summary>
/// Csf支持的语言 转换器
/// </summary>
public sealed class CsfLanguageJsonConverter : JsonConverter<int>
{
    /// <inheritdoc/>
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetInt32(),
            JsonTokenType.String => reader.GetString() switch
            {
                "en_US" => 0,
                "en_UK" => 1,
                "de" => 2,
                "fr" => 3,
                "es" => 4,
                "it" => 5,
                "jp" => 6,
                "Jabberwockie" => 7,
                "kr" => 8,
                "zh" => 9,
                _ => reader.GetString().ThrowNotSupportValue<string?, int>()
            },
            _ => reader.TokenType.ThrowNotSupportToken<int>()
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case 0:
                writer.WriteStringValue("en_US");
                break;

            case 1:
                writer.WriteStringValue("en_UK");
                break;

            case 2:
                writer.WriteStringValue("de");
                break;

            case 3:
                writer.WriteStringValue("fr");
                break;

            case 4:
                writer.WriteStringValue("es");
                break;

            case 5:
                writer.WriteStringValue("it");
                break;

            case 6:
                writer.WriteStringValue("jp");
                break;

            case 7:
                writer.WriteStringValue("Jabberwockie");
                break;

            case 8:
                writer.WriteStringValue("kr");
                break;

            case 9:
                writer.WriteStringValue("zh");
                break;

            default:
                writer.WriteNumberValue(value);
                break;
        }
    }
}
