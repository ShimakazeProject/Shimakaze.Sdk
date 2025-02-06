using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Models.V1;

/// <summary>
/// 
/// </summary>
/// <param name="Label"></param>
/// <param name="Values"></param>
[JsonConverter(typeof(CsfJsonLabelJsonConverter))]
public sealed record class CsfJsonLabel(string Label, List<CsfJsonValue> Values)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    public static implicit operator CsfJsonLabel(in CsfLabel v) => new(v.Name, [.. v.Values.Select(static i => (CsfJsonValue)i)]);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    public static implicit operator CsfLabel(in CsfJsonLabel v) => new(v.Label, [.. v.Values.Select(static i => (CsfValue)i)]);

    internal sealed class CsfJsonLabelJsonConverter : JsonConverter<CsfJsonLabel>
    {
        /// <inheritdoc/>
        public override CsfJsonLabel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var json = JsonElement.ParseValue(ref reader);
            json.IsKind(JsonValueKind.Object);

            var label = json.GetProperty("label").GetString();
            Debug.Assert(label is not null);

            if (!json.TryGetProperty("values", out var values))
            {
                return json.Deserialize<CsfJsonValue>(options) switch
                {
                    { Value: not null } value => new(label, [value]),
                    _ => new(label, [])
                };
            }

            values.IsKind(JsonValueKind.Array);

            var length = values.GetArrayLength();
            List<CsfJsonValue> result = new(length);
            for (int i = 0; i < length; i++)
            {
                switch (values[i])
                {
                    case { ValueKind: JsonValueKind.String } value:
                        var v = value.GetString();
                        Debug.Assert(v is not null);
                        result.Add(new(v, default));
                        break;
                    case { ValueKind: JsonValueKind.Array } value:
                        v = string.Join("\r\n", value.Deserialize<string[]>() ?? []);
                        result.Add(new(v, default));
                        break;
                    case { ValueKind: JsonValueKind.Object } value:
                        var a = value.Deserialize<CsfJsonValue>(options);
                        Debug.Assert(a is not null);
                        result.Add(a);
                        break;
                }
            }
            return new(label, result);

        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, CsfJsonLabel value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("label", value.Label);
            switch (value.Values.Count)
            {
                case 0:
                    writer.WriteNull("value");
                    break;
                case 1:
                    JsonSerializer.Serialize(writer, value.Values[0], options);
                    break;
                default:
                    {
                        writer.WriteStartArray("values");
                        foreach (var item in value.Values)
                        {
                            writer.WriteStartObject();
                            JsonSerializer.Serialize(writer, item, options);
                            writer.WriteEndObject();
                        }
                        writer.WriteEndArray();
                        break;
                    }
            }
            writer.WriteEndObject();
        }
    }
}
