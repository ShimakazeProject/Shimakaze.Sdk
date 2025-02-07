namespace Shimakaze.Sdk.Csf.Yaml;

/// <summary>
/// YamlConstants.
/// </summary>
internal static class YamlConstants
{
    /// <summary>
    /// LanguageList.
    /// </summary>
    public static readonly List<string> LanguageList =
    [
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
    ];

    /// <summary>
    /// SchemaUrls.
    /// </summary>
    public static class SchemaUrls
    {
        /// <summary>
        /// BASEURL.
        /// </summary>
        public const string BASEURL = "https://schema.shimakaze.org";

        public static class V1
        {
            /// <summary>
            /// Metadata.
            /// </summary>
            public const string Head = $"{BASEURL}/yaml/csf/v1.yaml#/definitions/head";

            /// <summary>
            /// V1.
            /// </summary>
            public const string Data = $"{BASEURL}/yaml/csf/v1.yaml#/definitions/data";
        }
    }
}
