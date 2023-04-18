using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json;

internal static class JsonConverterExtensions
{
    public static TJsonConverter Get<TJsonConverter>(this JsonSerializerOptions options)
        where TJsonConverter : JsonConverter
        => options.Converters.FirstOrDefault(
            i => typeof(TJsonConverter).IsInstanceOfType(i)
        ) as TJsonConverter
            ?? throw new NotSupportedException(
                $"""
                Cannot found converter {typeof(TJsonConverter).FullName}.
                Are you sure you added it on JsonSerializerOptions?
                """
            );
}
