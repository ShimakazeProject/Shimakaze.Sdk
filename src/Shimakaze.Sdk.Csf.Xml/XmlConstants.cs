using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Csf.Xml;

[ExcludeFromCodeCoverage]
internal class XmlConstants
{
    public static class SchemaUrls
    {
        public const string BASE_URL = "https://shimakazeproject.github.io/Schemas";
        public const string V1 = BASE_URL + "/xml/csf/v1.xsd";
    }
}