using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json.Converter.V2;

/// <inheritdoc cref="JsonSerializerOptions" />
public static class CsfJsonSerializerOptions
{
    /// <inheritdoc cref="JsonSerializerOptions.Converters" />
    public static FrozenSet<JsonConverter> Converters => new JsonConverter[]
    {
        new V1.CsfLanguageJsonConverter(),
        new V1.CsfSimpleValueJsonConverter(),
        new V1.CsfAdvancedValueJsonConverter(),
        new V1.CsfValueJsonConverter(),
        new CsfDataValueJsonConverter(),
        new CsfFileJsonConverter()
    }.ToFrozenSet();
}