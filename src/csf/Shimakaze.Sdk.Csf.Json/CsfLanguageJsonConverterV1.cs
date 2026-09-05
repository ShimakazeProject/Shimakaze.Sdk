using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// Csf 语言 Json 转换器
/// </summary>
public sealed class CsfLanguageJsonConverterV1 : CsfJsonConverter<CsfLanguage>
{
    /// <inheritdoc/>
    public override CsfLanguage Read(in JsonElement json, JsonSerializerOptions options) => json.ValueKind switch
    {
        JsonValueKind.Number => json.GetInt32(),
        JsonValueKind.String => CsfLanguage.Parse(json.GetString()),
        _ => throw new JsonException(),
    };

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CsfLanguage value, JsonSerializerOptions options)
    {
        if (value.Value is >= 0 and <= 9)
            writer.WriteStringValue(value.ToString());
        else
            writer.WriteNumberValue(value);
    }
}
