using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Converter.V1;

/// <inheritdoc cref="JsonSerializerOptions"/>
public static class CsfJsonSerializerOptions
{
    /// <inheritdoc cref="JsonSerializerOptions.Converters"/>
    public static IList<JsonConverter> Converters => new List<JsonConverter>()
    {
        new CsfLanguageJsonConverter(),
        new CsfMetadataJsonConverter(),
        new CsfSimpleValueJsonConverter(),
        new CsfAdvancedValueJsonConverter(),
        new CsfValueJsonConverter(),
        new CsfDataJsonConverter(),
        new CsfFileJsonConverter()
    };
}
