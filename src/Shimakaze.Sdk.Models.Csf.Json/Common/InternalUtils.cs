namespace Shimakaze.Sdk.Models.Csf.Json.Common;
internal static class InternalUtils
{
    public static JsonConverter<T> GetConverter<T>(this JsonSerializerOptions options)
        => options.GetConverter(typeof(T)) as JsonConverter<T> ?? throw new JsonException($"Cannot Find JsonConverter<{typeof(T)}>.");

    internal static T Read<T>(this JsonConverter<T> @this, ref Utf8JsonReader reader, JsonSerializerOptions options)
        => @this.Read(ref reader, typeof(T), options) ?? throw new JsonException($"Cannot Reads and converts the JSON to type {typeof(T)}.");

}
