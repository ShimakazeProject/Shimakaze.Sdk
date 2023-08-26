using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// JsonConstants.
/// </summary>
[ExcludeFromCodeCoverage]
internal class JsonConstants
{
    /// <summary>
    /// SchemaUrls.
    /// </summary>
    public static class SchemaUrls
    {
        /// <summary>
        /// BASEURL.
        /// </summary>
        public const string BASEURL = "https://shimakazeproject.github.io/Shimakaze.Sdk/schemas";

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