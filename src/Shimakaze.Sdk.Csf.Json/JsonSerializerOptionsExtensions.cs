using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json;

internal static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions Init(this JsonSerializerOptions? options, IEnumerable<JsonConverter> converters)
    {
        options ??= new();
        foreach (var item in converters)
            options.Converters.Add(item);

        return options;
    }
}