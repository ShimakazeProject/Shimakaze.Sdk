using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Converter.V1;

/// <summary>
/// Csf复杂值 转换器
/// </summary>
public sealed class CsfAdvancedValueJsonConverter : JsonConverter<CsfValue>
{
    /// <inheritdoc />
    public override CsfValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.TokenType.ThrowWhenNotToken(JsonTokenType.StartObject);
        string value = string.Empty;
        string? extra = null;

        while (reader.Read().ThrowWhenNull())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
                break;
            switch (reader.GetString()?.ToLowerInvariant())
            {
                case "value":
                    value = reader
                        .Read<CsfSimpleValueJsonConverter, string>(options)
                        .ThrowWhenNull();
                    break;

                case "extra":
                    extra = reader.ReadString();
                    break;
            }
        }

        return string.IsNullOrEmpty(extra) switch
        {
            true => new CsfValue(value),
            false => new CsfValue(value, extra)
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, CsfValue value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteProperty<CsfSimpleValueJsonConverter, string>("value", value.Value, options);
        if (value.HasExtra)
            writer.WriteString("extra", value.ExtraValue);
        writer.WriteEndObject();
    }
}