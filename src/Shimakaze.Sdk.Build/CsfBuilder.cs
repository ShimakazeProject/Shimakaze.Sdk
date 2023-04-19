using Microsoft.Build.Framework;
using Microsoft.Extensions.DependencyInjection;

using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Csf.Json.Serialization;
using Shimakaze.Sdk.Csf.Xml.Serialization;
using Shimakaze.Sdk.Csf.Yaml.Serialization;
using Shimakaze.Sdk.IO.Csf.Serialization;
using Shimakaze.Sdk.IO.Serialization;

using MSTask = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Build;

/// <summary>
/// Csf 构建器
/// </summary>
public sealed class CsfBuilder : MSTask
{
    /// <summary>
    /// 将要被处理的文件
    /// </summary>
    [Required]
    public required string InputPath { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    [Required]
    public required string Type { get; set; }

    /// <summary>
    /// 生成的文件
    /// </summary>
    [Required]
    public required string OutputPath { get; set; }

    /// <inheritdoc/>
    public override bool Execute()
    {
        using Stream stream = File.OpenRead(InputPath);
        using Stream output = File.Create(OutputPath);

        ServiceCollection services = new();
        services.AddSingleton<ISerializer<CsfDocument>>(new CsfSerializer(output));

        switch (Type.ToLowerInvariant())
        {
            case "jsonv1":
                services.AddSingleton<IDeserializer<CsfDocument?>>(new CsfJsonV1Deserializer(stream));
                break;
            case "json":
            case "jsonv2":
                services.AddSingleton<IDeserializer<CsfDocument?>>(new CsfJsonV2Deserializer(stream));
                break;
            case "xml":
            case "xmlv1":
                services.AddSingleton<IDeserializer<CsfDocument>>(new CsfXmlV1Deserializer(stream));
                break;
            case "yml":
            case "yaml":
            case "ymlv1":
            case "yamlv1":
                services.AddSingleton<IDeserializer<CsfDocument?>>(new CsfYamlV1Deserializer(stream));
                break;
            default:
                throw new NotSupportedException(Type);
        }
        using ServiceProvider provider = services.BuildServiceProvider();
        var csf = provider.GetRequiredService<IDeserializer<CsfDocument?>>().Deserialize()
            ?? throw new InvalidDataException("Cannot Deserialize the file content to Csf Document.");

        provider.GetRequiredService<ISerializer<CsfDocument>>().Serialize(csf);

        return true;
    }
}
