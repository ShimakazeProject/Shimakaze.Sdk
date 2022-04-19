namespace Shimakaze.Sdk.Models.Csf.Json;

internal class JsonConstants
{
    public static class SchemaUrls
    {
        public const string BASE_URL = "https://shimakazeproject.github.io/Schemas";
        public const string V1 = BASE_URL + "/json/csf/v1.json";
        public const string V2 = BASE_URL + "/json/csf/v2.json";
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