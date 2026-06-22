using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json;

internal static class Utf8JsonWriterExtensions
{
    public static void WriteProperty<T>(this Utf8JsonWriter writer, string propertyName, T value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(propertyName);
        JsonSerializer.Serialize(writer, value, options);
    }
}
