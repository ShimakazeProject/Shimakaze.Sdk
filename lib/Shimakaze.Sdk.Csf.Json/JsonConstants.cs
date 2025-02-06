using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// JsonConstants.
/// </summary>
internal static class JsonConstants
{
    /// <summary>
    /// SchemaUrls.
    /// </summary>
    public static class SchemaUrls
    {
        /// <summary>
        /// BASEURL.
        /// </summary>
        public const string BASEURL = "https://schema.shimakaze.org";

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
