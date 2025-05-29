using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Models.V1;

/// <summary>
/// 
/// </summary>
/// <param name="Version"></param>
/// <param name="Language"></param>
[JsonConverter(typeof(CsfJsonMetadataJsonConverter))]
public record struct CsfJsonMetadata(int Version, CsfJsonLanguage Language)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    public static implicit operator CsfJsonMetadata(in CsfMetadata v) => new(v.Version, v.Language);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    public static implicit operator CsfMetadata(in CsfJsonMetadata v) => new()
    {
        Version = v.Version,
        Language = v.Language,
    };

    /// <summary>
    /// 
    /// </summary>
    internal sealed class CsfJsonMetadataJsonConverter : JsonConverter<CsfJsonMetadata>
    {
        /// <inheritdoc/>
        public override CsfJsonMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var json = JsonElement.ParseValue(ref reader);
            json.IsKind(JsonValueKind.Object);

            return new(json.GetProperty("version").GetInt32(), json.GetProperty("language").Deserialize<CsfJsonLanguage>(options));
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, CsfJsonMetadata value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("version", value.Version);
            writer.WriteProperty("language", value.Language, options);
            writer.WriteEndObject();
        }
    }

}
