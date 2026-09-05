using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// Csf 数据 Json 转换器
/// </summary>
public sealed class CsfDataJsonConverterV1 : CsfJsonConverter<CsfData>
{
    /// <inheritdoc/>
    public override CsfData? Read(in JsonElement json, JsonSerializerOptions options)
    {
        if (!options.TryGetTypeInfo<CsfMetadata>(out var rjtiMetadata))
            throw new JsonException($"Cannot find JsonTypeInfo for type {typeof(CsfMetadata)}");
        if (!options.TryGetTypeInfo<List<CsfLabel>>(out var rjtiROLLabel))
            throw new JsonException($"Cannot find JsonTypeInfo for type {typeof(List<CsfLabel>)}");

        json.IsKind(JsonValueKind.Object);
        json.HasProtocol(1);

        return new(
            JsonSerializer.Deserialize(json.GetProperty("head"), rjtiMetadata),
            JsonSerializer.Deserialize(json.GetProperty("data"), rjtiROLLabel) ?? []);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CsfData value, JsonSerializerOptions options)
    {
        if (!options.TryGetTypeInfo<CsfMetadata>(out var rjtiMetadata))
            throw new JsonException($"Cannot find JsonTypeInfo for type {typeof(CsfMetadata)}");
        if (!options.TryGetTypeInfo<List<CsfLabel>>(out var rjtiROLLabel))
            throw new JsonException($"Cannot find JsonTypeInfo for type {typeof(List<CsfLabel>)}");

        writer.WriteStartObject();
        writer.WriteString("$schema", JsonConstants.SchemaUrls.V1);
        writer.WriteNumber("protocol", 1);
        writer.WritePropertyName("head");
        JsonSerializer.Serialize(writer, value.Metadata, rjtiMetadata);
        writer.WritePropertyName("data");
        JsonSerializer.Serialize(writer, value.Labels, rjtiROLLabel);
        writer.WriteEndObject();
    }
}
