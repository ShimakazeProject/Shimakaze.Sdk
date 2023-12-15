using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json;

internal static class Utf8JsonReaderExtensions
{
    public static T? Get<TJsonConverter, T>(this ref Utf8JsonReader reader, JsonSerializerOptions options)
        where TJsonConverter : JsonConverter<T>
    {
        return options.Get<TJsonConverter>().Read(ref reader, typeof(T), options);
    }
    public static T GetNotNull<TJsonConverter, T>(this ref Utf8JsonReader reader, JsonSerializerOptions options)
        where TJsonConverter : JsonConverter<T>
    {
        T? result = reader.Get<TJsonConverter, T>(options);
        CsfJsonAsserts.IsNotNull(result);
        return result;
    }

    public static T Read<TJsonConverter, T>(this ref Utf8JsonReader reader, JsonSerializerOptions options)
        where TJsonConverter : JsonConverter<T>
    {
        CsfJsonAsserts.IsNotEndOfStream(reader.Read());
        return reader.GetNotNull<TJsonConverter, T>(options);
    }

    public static int ReadInt32(this ref Utf8JsonReader reader)
    {
        CsfJsonAsserts.IsNotEndOfStream(reader.Read());
        CsfJsonAsserts.IsToken(JsonTokenType.Number, reader.TokenType);
        return reader.GetInt32();
    }

    public static string ReadString(this ref Utf8JsonReader reader)
    {
        string? result = reader.ReadStringOrNull();
        CsfJsonAsserts.IsNotNull(result);
        return result;
    }
    public static string? ReadStringOrNull(this ref Utf8JsonReader reader)
    {
        CsfJsonAsserts.IsNotEndOfStream(reader.Read());
        CsfJsonAsserts.IsToken(JsonTokenType.String, reader.TokenType);
        return reader.GetString();
    }
}
