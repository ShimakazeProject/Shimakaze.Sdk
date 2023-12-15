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
        CsfJsonAsserts.IsToken(JsonTokenType.StartObject, reader.TokenType);

        CsfMetadata? metadata = null;
        List<CsfData>? data = null;
        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
                break;
            switch (reader.GetString()?.ToLowerInvariant())
            {
                case "protocol":
                    CsfJsonAsserts.IsProtocol(1, reader.ReadInt32());
                    break;

                case "head":
                    metadata = reader.Read<CsfMetadataJsonConverter, CsfMetadata>(options);
                    break;

                case "data":
                    data = ReadDataList(ref reader, options);
                    break;

                default:
                    reader.Read();
                    break;
            }
        }

        CsfJsonAsserts.IsNotNull(metadata);
        CsfJsonAsserts.IsNotNull(data);
        return new(metadata.Value, data);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, CsfDocument value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("$schema", JsonConstants.SchemaUrls.V1);
        writer.WriteNumber("protocol", 1);
        writer.WriteProperty<CsfMetadataJsonConverter, CsfMetadata>("head", value.Metadata, options);

        writer.WriteStartArray("data");
        foreach (var item in value.Data)
            writer.WriteValue<CsfDataJsonConverter, CsfData>(item, options);

        writer.WriteEndArray();

        writer.WriteEndObject();
    }

    private static List<CsfData> ReadDataList(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        CsfJsonAsserts.IsNotEndOfStream(reader.Read());
        CsfJsonAsserts.IsToken(JsonTokenType.StartArray, reader.TokenType);
        List<CsfData> data = [];
        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndArray)
                break;
            data.Add(reader.GetNotNull<CsfDataJsonConverter, CsfData>(options));
        }
        return data;
    }
}