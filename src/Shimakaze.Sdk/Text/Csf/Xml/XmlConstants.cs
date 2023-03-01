namespace Shimakaze.Tools.Csf.Serialization.Xml;

internal class XmlConstants
{
    public static class SchemaUrls
    {
        public const string BASE_URL = "https://shimakazeproject.github.io/Schemas";
        public const string V1 = BASE_URL + "/xml/csf/v1.xsd";
    }

    public static readonly string[] LanguageList = new[] {
        "en_US",
        "en_UK",
        "de",
        "fr",
        "es",
        "it",
        "jp",
        "Jabberwockie",
        "kr",
        "zh"
    };
}