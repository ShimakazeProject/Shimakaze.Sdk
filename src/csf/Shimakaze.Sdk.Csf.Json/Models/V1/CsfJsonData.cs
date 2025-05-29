using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Models.V1;

/// <summary>
/// 
/// </summary>
/// <param name="Head"></param>
/// <param name="Data"></param>
[JsonConverter(typeof(CsfJsonDataJsonConverter))]
public sealed record class CsfJsonData(CsfJsonMetadata Head, List<CsfJsonLabel> Data)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    public static implicit operator CsfJsonData(in CsfData v) => new(v.Metadata, [.. v.Labels.Select(static i => (CsfJsonLabel)i)]);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    public static implicit operator CsfData(in CsfJsonData v) => new(v.Head, [.. v.Data.Select(static i => (CsfLabel)i)]);

    /// <summary>
    /// 
    /// </summary>
    public sealed class CsfJsonDataJsonConverter : JsonConverter<CsfJsonData>
    {
        /// <inheritdoc/>
        public override CsfJsonData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var json = JsonElement.ParseValue(ref reader);
            json.IsKind(JsonValueKind.Object);
            json.HasProtocol(1);
            var metadata = json.GetProperty("head").Deserialize<CsfJsonMetadata>(options);
            var labels = json.GetProperty("data").Deserialize<List<CsfJsonLabel>>(options);

            Debug.Assert(labels is not null);
            return new(metadata, labels);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, CsfJsonData value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("$schema", JsonConstants.SchemaUrls.V1);
            writer.WriteNumber("protocol", 1);
            writer.WriteProperty("head", value.Head, options);
            writer.WriteProperty("data", value.Data, options);
            writer.WriteEndObject();
        }
    }

}
