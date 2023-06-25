using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Converter.V1;

/// <summary>
/// Csf值 转换器
/// </summary>
public sealed class CsfValueJsonConverter : JsonConverter<CsfValue>
{
    /// <inheritdoc/>
    public override CsfValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String or JsonTokenType.StartArray => new CsfValue(
                reader
                    .Get<CsfSimpleValueJsonConverter, string>(options)
                    .ThrowWhenNull()
            ),
            JsonTokenType.StartObject => reader
                    .Get<CsfAdvancedValueJsonConverter, CsfValue>(options),
            _ => reader.TokenType.ThrowNotSupportToken<CsfValue>()
        };
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CsfValue value, JsonSerializerOptions options)
    {
        if (!value.HasExtra)
        {
            writer.WriteValue<CsfSimpleValueJsonConverter, string>(value.Value, options);
            return;
        }
        writer.WriteValue<CsfAdvancedValueJsonConverter, CsfValue>(value, options);
    }
}