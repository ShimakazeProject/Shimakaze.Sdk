using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Csf.Yaml;

internal static class YamlSerializerOptionsExtensions
{
    extension(YamlSerializerOptions options)
    {
        public bool TryGetConverter<T>(
            [NotNullWhen(true)] out T? converter)
            where T : YamlConverter
        {
            converter =
                options.Converters
                    .OfType<T>()
                    .FirstOrDefault();

            return converter is not null;
        }
    }
}
