using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Text.Csf.Json;

internal static class Utf8JsonWriterExtensions
{
    public static void WriteProperty<TJsonConverter, T>(this Utf8JsonWriter writer, string propertyName, T value, JsonSerializerOptions options)
        where TJsonConverter : JsonConverter<T>
    {
        writer.WritePropertyName(propertyName);
        writer.WriteValue<TJsonConverter, T>(value, options);
    }

    public static void WriteValue<TJsonConverter, T>(this Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        where TJsonConverter : JsonConverter<T>
    {
        options.Get<TJsonConverter>().Write(writer, value, options);
    }
}
