using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Models.V2;

/// <summary>
/// 
/// </summary>
/// <param name="Version"></param>
/// <param name="Language"></param>
/// <param name="Data"></param>
[JsonConverter(typeof(CsfJsonDataJsonConverter))]
public sealed record class CsfJsonData(int Version, V1.CsfJsonLanguage Language, Dictionary<string, List<CsfJsonValue>> Data)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    public static implicit operator CsfMetadata(in CsfJsonData v) => new()
    {
        Version = v.Version,
        Language = v.Language,
    };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    public static implicit operator CsfJsonData(in CsfData v) => new(v.Metadata.Version, v.Metadata.Language, v.Labels.ToDictionary(i => i.Name, i => i.Select(i => (CsfJsonValue)i).ToList()));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    public static implicit operator CsfData(in CsfJsonData v) => new(v, [.. v.Data.Select(static i => new CsfLabel(i.Key, [.. i.Value.Select(static i => (CsfValue)i)]))]);


    internal sealed class CsfJsonDataJsonConverter : JsonConverter<CsfJsonData>
    {
        /// <inheritdoc/>
        public override CsfJsonData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var json = JsonElement.ParseValue(ref reader);
            json.IsKind(JsonValueKind.Object);
            json.HasProtocol(2);
            var version = json.GetProperty("version").GetInt32();
            var language = json.GetProperty("language").Deserialize<V1.CsfJsonLanguage>(options);

            var labels = ReadData(json.GetProperty("data"), options);

            Debug.Assert(labels is not null);
            return new(version, language, labels);
        }

        private static Dictionary<string, List<CsfJsonValue>> ReadData(in JsonElement json, JsonSerializerOptions options)
        {
            Dictionary<string, List<CsfJsonValue>> result = [];
            using var properties = json.EnumerateObject();
            foreach (var property in properties)
            {
                switch (property.Value)
                {
                    case { ValueKind: JsonValueKind.Null }:
                        result.Add(property.Name, []);
                        break;
                    case { ValueKind: JsonValueKind.String }:
                    case { ValueKind: JsonValueKind.Array }:
                        var value = CsfJsonValue.CsfJsonValueJsonConverter.ReadValueData(property.Value);
                        Debug.Assert(value is not null);
                        result.Add(property.Name, [new(value, default)]);
                        break;
                    case { ValueKind: JsonValueKind.Object }:
                        if (property.Value.TryGetProperty("values", out var values))
                        {
                            values.IsKind(JsonValueKind.Array);

                            var length = values.GetArrayLength();
                            List<CsfJsonValue> list = new(length);
                            for (int i = 0; i < length; i++)
                            {
                                switch (values[i])
                                {
                                    case { ValueKind: JsonValueKind.String }:
                                    case { ValueKind: JsonValueKind.Array }:
                                        value = CsfJsonValue.CsfJsonValueJsonConverter.ReadValueData(property.Value);
                                        Debug.Assert(value is not null);
                                        result.Add(property.Name, [new(value, default)]);
                                        break;
                                    case { ValueKind: JsonValueKind.Object } a:
                                        var c = a.Deserialize<CsfJsonValue>(options);
                                        Debug.Assert(c is not null);
                                        list.Add(c);
                                        break;
                                }
                            }

                            result.Add(property.Name, list);
                        }
                        else
                        {
                            if (property.Value.Deserialize<CsfJsonValue>(options) is { Value: not null } v)
                                result.Add(property.Name, [v]);
                            else
                                result.Add(property.Name, []);
                        }
                        break;
                }

                // var values = property.Value.Deserialize<List<CsfJsonValue>>();
                // Debug.Assert(values is not null);
                // result.Add(property.Name, values);
            }
            return result;
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, CsfJsonData value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("$schema", JsonConstants.SchemaUrls.V2);
            writer.WriteNumber("protocol", 2);
            writer.WriteNumber("version", value.Version);
            writer.WriteProperty("language", value.Language, options);
            writer.WriteStartObject("data");
            foreach (var item in value.Data)
            {
                writer.WritePropertyName(item.Key);
                switch (item.Value.Count)
                {
                    case 0:
                        writer.WriteNullValue();
                        break;
                    case 1:
                        JsonSerializer.Serialize(writer, item.Value[0], options);
                        break;
                    default:
                        {
                            writer.WriteStartArray("values");
                            foreach (var i in item.Value)
                                JsonSerializer.Serialize(writer, i, options);
                            writer.WriteEndArray();
                            break;
                        }
                }
            }
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
    }
};
