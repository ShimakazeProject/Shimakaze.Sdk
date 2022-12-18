namespace Shimakaze.Sdk.Text.Json.Csf;

/// <summary>
/// JsonConstants.
/// </summary>
internal class JsonConstants
{
    /// <summary>
    /// LanguageList.
    /// </summary>
    public static readonly string[] LanguageList = new[]
    {
    "en_US",
    "en_UK",
    "de",
    "fr",
    "es",
    "it",
    "jp",
    "Jabberwockie",
    "kr",
    "zh",
  };

    /// <summary>
    /// SchemaUrls.
    /// </summary>
    public static class SchemaUrls
    {
        /// <summary>
        /// BASEURL.
        /// </summary>
        public const string BASEURL = "https://shimakazeproject.github.io/Schemas";

        /// <summary>
        /// V1.
        /// </summary>
        public const string V1 = $"{BASEURL}/json/csf/v1.json";

        /// <summary>
        /// V2.
        /// </summary>
        public const string V2 = $"{BASEURL}/json/csf/v2.json";
    }
}
