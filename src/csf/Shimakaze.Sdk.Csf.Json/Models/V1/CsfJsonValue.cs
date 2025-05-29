using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Models.V1;

/// <summary>
/// 
/// </summary>
/// <param name="Value"></param>
/// <param name="Extra"></param>
[JsonConverter(typeof(CsfJsonValueJsonConverter))]
public sealed record class CsfJsonValue(string Value, string? Extra)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    public static implicit operator CsfJsonValue(in CsfValue v) => new(v.Value, v.Extra);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    public static implicit operator CsfValue(in CsfJsonValue v) => new(v.Value, v.Extra);

    internal sealed class CsfJsonValueJsonConverter : JsonConverter<CsfJsonValue>
    {
        public override CsfJsonValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var json = JsonElement.ParseValue(ref reader);
            json.IsKind(JsonValueKind.Object);

            var value = json.GetProperty("value") switch
            {
                { ValueKind: JsonValueKind.String } str => str.GetString(),
                { ValueKind: JsonValueKind.Array } arr => string.Join("\r\n", arr.Deserialize<string[]>() ?? []),
                _ => null,
            };
            if (value is null)
                return default;
            else if (json.TryGetProperty("extra", out var extra))
                return new(value, extra.GetString());
            else
                return new(value, default);
        }

        public override void Write(Utf8JsonWriter writer, CsfJsonValue value, JsonSerializerOptions options)
        {
            writer.WritePropertyName("value");
            if (value.Value.Contains('\n'))
            {
                writer.WriteStartArray();

                foreach (var line in value.Value.Split('\n'))
                    writer.WriteStringValue(line.TrimEnd('\r'));

                writer.WriteEndArray();
            }
            else
            {
                writer.WriteStringValue(value.Value);
            }

            if (value.Extra is not null)
            {
                writer.WriteString("extra", value.Extra);
            }
        }
    }
};
