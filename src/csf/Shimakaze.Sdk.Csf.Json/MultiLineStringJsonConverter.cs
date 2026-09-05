using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// 多行字符串 Json 转换器
/// </summary>
public sealed class MultiLineStringJsonConverter : CsfJsonConverter<string>
{
    /// <inheritdoc/>
    public override string? Read(in JsonElement json, JsonSerializerOptions options)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Array:
                if (!options.TryGetTypeInfo<IEnumerable<string>>(out var typeInfo))
                    throw new JsonException($"Cannot find JsonTypeInfo for type {typeof(IEnumerable<string>)}");

                return string.Join("\r\n", json.Deserialize(typeInfo) ?? []);
            default:
                if (json.GetString() is { } szValue)
                    return szValue;
                return default;
        }
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        if (!value.Contains('\n'))
        {
            writer.WriteStringValue(value);
            return;
        }

        writer.WriteStartArray();

        foreach (ReadOnlySpan<char> tmp in value.Split('\n'))
        {
            var line = tmp;
            if (line.EndsWith('\r'))
                line = line[..^1];

            writer.WriteStringValue(line);
        }

        writer.WriteEndArray();
    }
}
