using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Json;

/// <summary>
/// 
/// </summary>
[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Serialization,
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    WriteIndented = true)]
[JsonSerializable(typeof(CsfData))]
[JsonSerializable(typeof(CsfValue))]
[JsonSerializable(typeof(CsfMetadata))]
[JsonSerializable(typeof(CsfLanguage))]
public sealed partial class CsfJsonV2JsonSerializerContext : CsfJsonSerializerContext
{
    /// <inheritdoc/>
    protected override void InitializeOptions(JsonSerializerOptions options)
    {
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.Converters.Add(new CsfDataJsonConverterV2());
        options.Converters.Add(new CsfValueJsonConverterV2());
        options.Converters.Add(new CsfMetadataJsonConverterV1());
        options.Converters.Add(new CsfLanguageJsonConverterV1());
        options.Converters.Add(new MultiLineStringJsonConverter());
    }
}
