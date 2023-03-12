﻿using System.Text.Json;
using System.Text.Json.Serialization;

using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Text.Csf.Json.Converter.V1;

/// <summary>
/// Csf元数据 转换器
/// </summary>
public sealed class CsfMetadataJsonConverter : JsonConverter<CsfMetadata>
{
    /// <inheritdoc/>
    public override CsfMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.TokenType.ThrowWhenNotToken(JsonTokenType.StartObject);

        CsfMetadata metadata = new(0, 0);
        while (reader.Read().ThrowWhenNull())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
                break;

            switch (reader.GetString()?.ToLowerInvariant())
            {
                case "version":
                    metadata.Version = reader.ReadInt32();
                    break;

                case "language":
                    metadata.Language = reader.Read<CsfLanguageJsonConverter, int>(options);
                    break;

                default:
                    reader.Read().ThrowWhenNull();
                    break;
            }
        }
        return metadata;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CsfMetadata value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("version", value.Version);
        writer.WriteProperty<CsfLanguageJsonConverter, int>("language", value.Language, options);
        writer.WriteEndObject();
    }
}
