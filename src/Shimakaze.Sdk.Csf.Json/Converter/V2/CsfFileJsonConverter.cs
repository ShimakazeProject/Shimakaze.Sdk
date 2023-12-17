using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Converter.V2;

/// <summary>
/// Csf文件 转换器
/// </summary>
public sealed class CsfFileJsonConverter : JsonConverter<CsfDocument>
{
    /// <inheritdoc />
    public override CsfDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        CsfJsonAsserts.IsToken(JsonTokenType.StartObject, reader.TokenType);

        CsfMetadata metadata = new(0, 0);
        List<CsfData>? data = null;
        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
                break;
            switch (reader.GetString()?.ToLowerInvariant())
            {
                case "protocol":
                    CsfJsonAsserts.IsProtocol(2, reader.ReadInt32());
                    break;

                case "version":
                    metadata.Version = reader.ReadInt32();
                    break;

                case "language":
                    metadata.Language = reader.Read<V1.CsfLanguageJsonConverter, int>(options);
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
        return new(metadata, data);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, CsfDocument value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("$schema", JsonConstants.SchemaUrls.V2);
        writer.WriteNumber("protocol", 2);
        writer.WriteNumber("version", value.Metadata.Version);
        writer.WriteProperty<V1.CsfLanguageJsonConverter, int>("language", value.Metadata.Language, options);

        writer.WriteStartObject("data");
        foreach (var item in value.Data)
            writer.WriteProperty<CsfDataValueJsonConverter, IList<CsfValue>>(item.LabelName, item.Values, options);

        writer.WriteEndObject();
        writer.WriteEndObject();
    }

    private static List<CsfData> ReadDataList(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        CsfJsonAsserts.IsNotEndOfStream(reader.Read());
        CsfJsonAsserts.IsToken(JsonTokenType.StartObject, reader.TokenType);
        List<CsfData> list = [];

        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
                break;
            var label = reader.GetString();
            CsfJsonAsserts.IsNotNull(label);
            list.Add(new(label)
            {
                Values = [.. reader.Read<CsfDataValueJsonConverter, IList<CsfValue>>(options)]
            });
        }

        return list;
    }
}