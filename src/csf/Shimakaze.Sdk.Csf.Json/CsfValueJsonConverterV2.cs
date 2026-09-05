using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// Csf 值 Json 转换器 V2
/// </summary>
public sealed class CsfValueJsonConverterV2 : CsfJsonConverter<CsfValue>
{
    /// <inheritdoc/>
    public override CsfValue? Read(in JsonElement json, JsonSerializerOptions options)
    {
        if (!options.TryGetConverter<MultiLineStringJsonConverter>(out var converter))
            converter = new();

        if (json.ValueKind is JsonValueKind.Null)
            return null;

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

        string? result = converter.Read(json, options);

        return result is null
            ? null
            : new(result, null);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CsfValue value, JsonSerializerOptions options)
    {
        if (!options.TryGetConverter<MultiLineStringJsonConverter>(out var converter))
            converter = new();

        if (value.Extra is null)
        {
            converter.Write(writer, value.Value, options);
            return;
        }

        writer.WriteStartObject();

        writer.WritePropertyName("value");
        converter.Write(writer, value.Value, options);

        writer.WriteString("extra", value.Extra);

        writer.WriteEndObject();
    }
}
