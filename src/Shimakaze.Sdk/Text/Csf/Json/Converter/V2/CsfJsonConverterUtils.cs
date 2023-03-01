using System.Text.Encodings.Web;
using System.Text.Json;

namespace Shimakaze.Sdk.Text.Csf.Json.Converter.V2;

/// <summary>
/// CsfJsonConverterUtils.
/// </summary>
public static class CsfJsonConverterUtils
{
    /// <summary>
    /// Gets create New Instance Always.
    /// </summary>
    public static JsonSerializerOptions CsfJsonSerializerOptions
    {
        get
        {
            JsonSerializerOptions options = new()
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

                // IncludeFields = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                AllowTrailingCommas = true,
            };
            options.Converters.Add(new CsfStructJsonConverter());
            options.Converters.Add(new CsfLabelsJsonConverter());
            options.Converters.Add(new CsfLabelJsonConverter());
            options.Converters.Add(new V1.CsfValuesJsonConverter());
            options.Converters.Add(new V1.CsfValueJsonConverter());
            options.Converters.Add(new Common.MultiLineStringJsonConverter());
            return options;
        }
    }
}
