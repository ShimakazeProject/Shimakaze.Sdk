using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json;

/// <inheritdoc/>
public abstract class CsfJsonConverter<T> : JsonConverter<T>
{
    /// <inheritdoc/>
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var json = JsonElement.ParseValue(ref reader);
        return Read(json, options);
    }

    /// <inheritdoc cref="Read(ref Utf8JsonReader, Type, JsonSerializerOptions)"/>
    /// <param name="json"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public abstract T? Read(in JsonElement json, JsonSerializerOptions options);
}
