using System.Text.Json.Serialization;
using Shimakaze.Sdk.Inilyn.SourceMapping;

namespace Shimakaze.Sdk.Inilyn;

/// <summary>
/// <see cref="SourceMap"/> 的源生成 JSON 序列化上下文。
/// </summary>
[JsonSourceGenerationOptions]
[JsonSerializable(typeof(SourceMap))]
public sealed partial class SourceMapJsonContext : JsonSerializerContext;
