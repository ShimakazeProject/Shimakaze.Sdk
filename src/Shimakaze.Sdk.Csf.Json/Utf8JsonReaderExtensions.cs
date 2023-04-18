using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json;

internal static class Utf8JsonReaderExtensions
{
    public static string? ReadString(this ref Utf8JsonReader reader)
    {
        reader.Read().ThrowWhenFalse();
        reader.TokenType.ThrowWhenNotToken(JsonTokenType.String);
        return reader.GetString();
    }

    public static int ReadInt32(this ref Utf8JsonReader reader)
    {
        reader.Read().ThrowWhenFalse();
        reader.TokenType.ThrowWhenNotToken(JsonTokenType.Number);
        return reader.GetInt32();
    }

    public static T? Read<TJsonConverter, T>(this ref Utf8JsonReader reader, JsonSerializerOptions options)
        where TJsonConverter : JsonConverter<T>
    {
        reader.Read().ThrowWhenFalse();
        return reader.Get<TJsonConverter, T>(options);
    }

    public static T? Get<TJsonConverter, T>(this ref Utf8JsonReader reader, JsonSerializerOptions options)
        where TJsonConverter : JsonConverter<T>
    {
        return options.Get<TJsonConverter>().Read(ref reader, typeof(T), options);
    }
}
