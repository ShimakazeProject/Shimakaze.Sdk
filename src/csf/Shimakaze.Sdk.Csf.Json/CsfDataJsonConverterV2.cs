using System.Text.Json;


namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// Csf 数据 Json 转换器 V2
/// </summary>
public sealed class CsfDataJsonConverterV2 : CsfJsonConverter<CsfData>
{
    /// <inheritdoc/>
    public override CsfData? Read(in JsonElement json, JsonSerializerOptions options)
    {
        if (!options.TryGetTypeInfo<CsfLanguage>(out var jtiLanguage))
            throw new JsonException($"Cannot find JsonTypeInfo for type {typeof(CsfLanguage)}");

        json.IsKind(JsonValueKind.Object);
        json.HasProtocol(2);

        int version = json.GetProperty("version").GetInt32();
        var language = JsonSerializer.Deserialize(json.GetProperty("language"), jtiLanguage);

        return new(
            new()
            {
                Version = version,
                Language = language
            },
            ReadData(json.GetProperty("data"), options));
    }

    private static List<CsfLabel> ReadData(in JsonElement json, JsonSerializerOptions options)
    {
        if (!options.TryGetConverter<CsfValueJsonConverterV2>(out var converter))
            converter = new();

        List<CsfLabel> result = [];

        using var enumerator = json.EnumerateObject();
        foreach (var property in enumerator)
        {
            switch (property.Value.ValueKind)
            {
                case JsonValueKind.Object when property.Value.TryGetProperty("values", out var values):
                    values.IsKind(JsonValueKind.Array);

                    using (var enumerator2 = values.EnumerateArray())
                    {
                        result.Add(new(
                            property.Name,
                            [.. enumerator2.Select(i => converter.Read(i, options) ?? new(string.Empty, null))]));
                    }

                    break;

                default:
                    var value = converter.Read(property.Value, options);

                    result.Add(new(
                        property.Name,
                        value is null ? [] : [value]));

                    break;
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CsfData value, JsonSerializerOptions options)
    {
        if (!options.TryGetTypeInfo<CsfLanguage>(out var jtiLanguage))
            throw new JsonException($"Cannot find JsonTypeInfo for type {typeof(CsfLanguage)}");

        if (!options.TryGetConverter<CsfValueJsonConverterV2>(out var converter))
            converter = new();

        writer.WriteStartObject();
        writer.WriteString("$schema", JsonConstants.SchemaUrls.V2);
        writer.WriteNumber("protocol", 2);
        writer.WriteNumber("version", value.Metadata.Version);

        writer.WritePropertyName("language");
        JsonSerializer.Serialize(writer, value.Metadata.Language, jtiLanguage);

        writer.WriteStartObject("data");

        foreach (var item in value.Labels)
        {
            writer.WritePropertyName(item.Name);

            switch (item.Count)
            {
                case 0:
                    writer.WriteNullValue();
                    break;

                case 1:
                    converter.Write(writer, item[0], options);
                    break;

                default:
                    writer.WriteStartObject();
                    writer.WritePropertyName("values");

                    writer.WriteStartArray();

                    foreach (var v in item)
                        converter.Write(writer, v, options);

                    writer.WriteEndArray();
                    writer.WriteEndObject();

                    break;
            }
        }

        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}
