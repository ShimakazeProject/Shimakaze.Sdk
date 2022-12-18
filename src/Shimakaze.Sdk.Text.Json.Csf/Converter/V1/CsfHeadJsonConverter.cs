﻿using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Text.Json.Csf.Converter.V1;

/// <summary>
/// CsfHeadJsonConverter.
/// </summary>
public class CsfHeadJsonConverter : JsonConverter<CsfMetadata>
{
    /// <inheritdoc/>
    public override CsfMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        CsfMetadata result = default;
        result.Identifier = CsfConstants.CsfFlagRaw;
        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            switch (reader.GetString()?.ToLower() ?? throw new JsonException())
            {
                case "version":
                    reader.Read();
                    result.Version = reader.GetInt32();
                    break;

                case "language":
                    reader.Read();
                    if (reader.TokenType is JsonTokenType.Number)
                    {
                        result.Language = reader.GetInt32();
                    }
                    else if (reader.TokenType is JsonTokenType.String)
                    {
                        string code = reader.GetString() ?? throw new JsonException();
                        for (result.Language = 0; result.Language < JsonConstants.LanguageList.Length; result.Language++)
                        {
                            if (JsonConstants.LanguageList[result.Language].Equals(code))
                            {
                                break;
                            }
                        }
                    }

                    break;

                default:
                    throw new JsonException();
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, CsfMetadata value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("version", value.Version);
        if (value.Language < JsonConstants.LanguageList.Length)
        {
            writer.WriteString("language", JsonConstants.LanguageList[value.Language]);
        }
        else
        {
            writer.WriteNumber("language", value.Language);
        }

        writer.WriteEndObject();
    }
}
