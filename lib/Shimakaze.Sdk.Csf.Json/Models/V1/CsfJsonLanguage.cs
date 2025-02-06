using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Models.V1;

/// <inheritdoc cref="CsfLanguage"/>
[JsonConverter(typeof(CsfJsonLanguageJsonConverter))]
public record struct CsfJsonLanguage(int Value)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="language"></param>
    public static implicit operator int(in CsfJsonLanguage language) => language.Value;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator CsfJsonLanguage(in int value) => new(value);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="language"></param>
    public static implicit operator CsfJsonLanguage(in CsfLanguage language) => language.Value;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="language"></param>
    public static implicit operator CsfLanguage(in CsfJsonLanguage language) => language.Value;

    internal sealed class CsfJsonLanguageJsonConverter : JsonConverter<CsfJsonLanguage>
    {
        /// <inheritdoc/>
        public override CsfJsonLanguage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var json = JsonElement.ParseValue(ref reader);

            return json.ValueKind switch
            {
                JsonValueKind.Number => json.GetInt32(),
                JsonValueKind.String => CsfLanguage.Parse(json.GetString()),
                _ => 0
            };
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, CsfJsonLanguage value, JsonSerializerOptions options)
        {
            if (value.Value is >= 0 and <= 9)
                writer.WriteStringValue(((CsfLanguage)value).ToString());
            else
                writer.WriteNumberValue(value);
        }
    }

}
