
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Shimakaze.Sdk;

/// <summary>
/// JsonSerializerExtensions.
/// </summary>
internal static class JsonSerializerExtensions
{
    /// <summary>
    /// GetConverter.
    /// </summary>
    /// <typeparam name="T">T.</typeparam>
    /// <param name="options">options.</param>
    /// <returns><see cref="JsonConverter{T}"/>.</returns>
    /// <exception cref="JsonException">Cannot Find <see cref="JsonConverter{T}"/>.</exception>
    public static JsonConverter<T> GetConverter<T>(this JsonSerializerOptions options)
        => options.GetConverter(typeof(T)) as JsonConverter<T> ?? throw new JsonException($"Cannot Find JsonConverter<{typeof(T)}>.");

    /// <summary>
    /// Read.
    /// </summary>
    /// <typeparam name="T">T.</typeparam>
    /// <param name="this"><see cref="JsonConverter{T}"/>.</param>
    /// <param name="reader">reader.</param>
    /// <param name="options">options.</param>
    /// <returns>T object.</returns>
    /// <exception cref="JsonException">Cannot Reads and converts the JSON to type T.</exception>
    internal static T Read<T>(this JsonConverter<T> @this, ref Utf8JsonReader reader, JsonSerializerOptions options)
        => @this.Read(ref reader, typeof(T), options) ?? throw new JsonException($"Cannot Reads and converts the JSON to type {typeof(T)}.");
}
