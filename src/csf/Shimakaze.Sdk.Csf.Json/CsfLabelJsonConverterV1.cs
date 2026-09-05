using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// Csf 标签 Json 转换器
/// </summary>
public sealed class CsfLabelJsonConverterV1 : CsfJsonConverter<CsfLabel>
{
    /// <inheritdoc/>
    public override CsfLabel? Read(in JsonElement json, JsonSerializerOptions options)
    {
        json.IsKind(JsonValueKind.Object);

        string label = json.GetProperty("label").GetString() ?? string.Empty;

        if (json.TryGetProperty("values", out var values))
        {
            if (!options.TryGetConverter<CsfValueJsonConverterV1>(out var converter))
                converter = new();

            using var enumerator = values.EnumerateArray();
            return new(label, [.. enumerator.Select(i => converter.Read(i, options) ?? new("", null))]);
        }

        if (!options.TryGetConverter<CsfValueJsonConverterV1>(out var valueConverter))
            valueConverter = new();

        CsfValue? value = null;

        if (json.TryGetProperty("value", out var jValue))
            value = valueConverter.Read(jValue, options);

        if (value is null)
            return new(label, []);

        if (json.TryGetProperty("extra", out var extra))
            value = value with { Extra = extra.GetString() };

        return new(label, [value]);
    }


    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CsfLabel value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("label", value.Name);

        if (value.Count == 0)
        {
            writer.WriteNull("value");
            writer.WriteEndObject();
            return;
        }

        if (value.Count == 1)
        {
            writer.WritePropertyName("value");

            if (!options.TryGetConverter<CsfValueJsonConverterV1>(out var converter))
                converter = new();

            converter.Write(writer, value[0] with { Extra = null }, options);

            if (value[0].Extra is not null)
                writer.WriteString("extra", value[0].Extra);

            writer.WriteEndObject();
            return;
        }

        writer.WritePropertyName("values");

        writer.WriteStartArray();

        if (!options.TryGetConverter<CsfValueJsonConverterV1>(out var valueConverter))
            valueConverter = new();

        foreach (var item in value)
            valueConverter.Write(writer, item, options);

        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}
