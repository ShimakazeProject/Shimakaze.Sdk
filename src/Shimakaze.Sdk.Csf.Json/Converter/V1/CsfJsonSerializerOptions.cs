using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Converter.V1;

/// <inheritdoc cref="JsonSerializerOptions" />
public static class CsfJsonSerializerOptions
{
    /// <inheritdoc cref="JsonSerializerOptions.Converters" />
    public static FrozenSet<JsonConverter> Converters => new JsonConverter[]
    {
        new CsfLanguageJsonConverter(),
        new CsfMetadataJsonConverter(),
        new CsfSimpleValueJsonConverter(),
        new CsfAdvancedValueJsonConverter(),
        new CsfValueJsonConverter(),
        new CsfDataJsonConverter(),
        new CsfFileJsonConverter()
    }.ToFrozenSet();
}