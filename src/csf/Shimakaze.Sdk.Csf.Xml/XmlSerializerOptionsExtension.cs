using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Csf.Xml;

internal static class XmlSerializerOptionsExtension
{
    extension(XmlSerializerOptions options)
    {
        public bool TryGetConverter<TConverter>([NotNullWhen(true)] out TConverter? converter)
            where TConverter : XmlConverter
        {
            converter = options.Converters.FirstOrDefault(i => i is TConverter) as TConverter;
            return converter is not null;
        }
    }
}
