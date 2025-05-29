using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Models.V2;

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

            if (json.ValueKind is JsonValueKind.Null)
                return default;

            if (ReadValueData(json) is { } singleValue)
                return new(singleValue, default);

            json.IsKind(JsonValueKind.Object);

            singleValue = json.GetProperty("value") switch
            {
                { ValueKind: JsonValueKind.String } str => str.GetString(),
                { ValueKind: JsonValueKind.Array } arr => string.Join("\r\n", arr.Deserialize<string[]>() ?? []),
                _ => null,
            };
            Debug.Assert(singleValue is not null);
            if (json.TryGetProperty("extra", out var extra))
                return new(singleValue, extra.GetString());
            else
                return new(singleValue, default);
        }

        internal static string? ReadValueData(in JsonElement json) => json switch
        {
            { ValueKind: JsonValueKind.String } str => str.GetString(),
            { ValueKind: JsonValueKind.Array } arr => string.Join("\r\n", arr.Deserialize<string[]>() ?? []),
            _ => null,
        };

        public override void Write(Utf8JsonWriter writer, CsfJsonValue value, JsonSerializerOptions options)
        {
            if (value.Extra is null)
            {
                WriteValueData(writer, value);
            }
            else
            {
                writer.WriteStartObject();
                writer.WritePropertyName("value");
                WriteValueData(writer, value);
                writer.WriteString("extra", value.Extra);
                writer.WriteEndObject();
            }
        }

        private static void WriteValueData(Utf8JsonWriter writer, CsfJsonValue value)
        {
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
        }
    }
};
