using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// Csf 元数据 Json 转换器
/// </summary>
public sealed class CsfMetadataJsonConverterV1 : CsfJsonConverter<CsfMetadata>
{
    /// <inheritdoc/>
    public override CsfMetadata Read(in JsonElement json, JsonSerializerOptions options)
    {
        if (!options.TryGetTypeInfo<CsfLanguage>(out var typeInfo))
            throw new JsonException($"Cannot find JsonTypeInfo for type {typeof(CsfMetadata)}");

        json.IsKind(JsonValueKind.Object);

        return new()
        {
            Version = json.GetProperty("version").GetInt32(),
            Language = JsonSerializer.Deserialize(json.GetProperty("language"), typeInfo)
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CsfMetadata value, JsonSerializerOptions options)
    {
        if (!options.TryGetTypeInfo<CsfLanguage>(out var typeInfo))
            throw new JsonException($"Cannot find JsonTypeInfo for type {typeof(CsfMetadata)}");

        writer.WriteStartObject();
        writer.WriteNumber("version", value.Version);
        writer.WritePropertyName("language");
        JsonSerializer.Serialize(writer, value.Language, typeInfo);
        writer.WriteEndObject();
    }
}
