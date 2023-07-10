using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Converter.V1;

/// <summary>
/// Csf文件 转换器
/// </summary>
public sealed class CsfFileJsonConverter : JsonConverter<CsfDocument>
{
    /// <inheritdoc />
    public override CsfDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.TokenType.ThrowWhenNotToken(JsonTokenType.StartObject);

        CsfMetadata? metadata = null;
        List<CsfData>? data = null;
        while (reader.Read().ThrowWhenNull())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
                break;
            switch (reader.GetString()?.ToLowerInvariant())
            {
                case "protocol":
                    reader.ReadInt32().Equals(1).ThrowWhenFalse($"Cannot Support Protocol \"{reader.GetInt32()}\"");
                    break;

                case "head":
                    metadata = reader.Read<CsfMetadataJsonConverter, CsfMetadata>(options).ThrowWhenNull();
                    break;

                case "data":
                    data = ReadDataList(ref reader, options);
                    break;

                default:
                    reader.Read().ThrowWhenNull();
                    break;
            }
        }

        metadata.ThrowWhenNull();
        data.ThrowWhenNull();
        return new(metadata.Value, data);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, CsfDocument value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("$schema", JsonConstants.SchemaUrls.V1);
        writer.WriteProperty<CsfMetadataJsonConverter, CsfMetadata>("head", value.Metadata, options);

        writer.WriteStartArray("data");
        foreach (var item in value.Data)
            writer.WriteValue<CsfDataJsonConverter, CsfData>(item, options);

        writer.WriteEndArray();

        writer.WriteEndObject();
    }

    private static List<CsfData> ReadDataList(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        reader.Read().ThrowWhenNull();
        reader.TokenType.ThrowWhenNotToken(JsonTokenType.StartArray);
        List<CsfData> data = new();
        while (reader.Read().ThrowWhenNull())
        {
            if (reader.TokenType is JsonTokenType.EndArray)
                break;
            data.Add(reader.Get<CsfDataJsonConverter, CsfData>(options).ThrowWhenNull());
        }
        return data;
    }
}