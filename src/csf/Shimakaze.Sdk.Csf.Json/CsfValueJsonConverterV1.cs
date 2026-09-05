using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// Csf 值 Json 转换器
/// </summary>
public sealed class CsfValueJsonConverterV1 : CsfJsonConverter<CsfValue>
{
    /// <inheritdoc/>
    public override CsfValue? Read(in JsonElement json, JsonSerializerOptions options)
    {
        if (!options.TryGetConverter<MultiLineStringJsonConverter>(out var converter))
            converter = new();

        if (json.ValueKind is JsonValueKind.Object)
        {
            string? value = converter.Read(json.GetProperty("value"), options);
            if (value is null)
                return null;

            string? extra = json.TryGetProperty("extra", out var jExtra)
                ? jExtra.GetString()
                : null;

            return new(value, extra);
        }

        string? text = converter.Read(json, options);
        return text is null ? null : new(text, null);
    }


    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CsfValue value, JsonSerializerOptions options)
    {
        if (value.Extra is null)
        {
            WriteValue(writer, value.Value, options);
            return;
        }

        writer.WriteStartObject();
        writer.WritePropertyName("value");
        WriteValue(writer, value.Value, options);
        writer.WriteString("extra", value.Extra);
        writer.WriteEndObject();
    }


    private static void WriteValue(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        if (!options.TryGetConverter<MultiLineStringJsonConverter>(out var converter))
            converter = new();

        converter.Write(writer, value, options);
    }
}
