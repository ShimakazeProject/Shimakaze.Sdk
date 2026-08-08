using System.Text.Json.Serialization;
using Shimakaze.Sdk.Inilyn.SourceMapping;

namespace Shimakaze.Sdk.Inilyn;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(SourceMap))]
public sealed partial class SourceMapJsonContext : JsonSerializerContext;
