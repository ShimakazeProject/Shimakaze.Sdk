using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
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
    public required ITaskItem[] SourceFiles { get; set; }

    /// <summary>
    /// 生成的目标文件
    /// </summary>
    [Output]
    public ITaskItem[] OutputFiles { get; set; } = Array.Empty<ITaskItem>();

    const string Metadata_Destination = "Destination";
    const string Metadata_Tag = "Tag";

    private void ExecuteOne(string inputPath, string outputPath, ref IList<ITaskItem> items)
    {
        items.Add(new TaskItem(outputPath));
    }

    /// <inheritdoc/>
    public override bool Execute()
    {
        Log.LogMessage("Generating CSF File...");

        ServiceCollection services = new();
        foreach (var file in SourceFiles)
        {
            var dest = file.GetMetadata(Metadata_Destination);
            var tag = file.GetMetadata(Metadata_Tag);
            if (!dest.CreateParentDirectory(Log))
                return false;

            services.Clear();
            using Stream stream = File.OpenRead(file.ItemSpec);
            using Stream output = File.Create(dest);
            services.AddSingleton<ISerializer<CsfDocument>>(new CsfSerializer(output));

            switch (tag.ToLowerInvariant())
            {
                case "jsonv1":
                    Log.LogWarning(
                        "Shimakaze.Sdk.Csf",
                        "CSF0001",
                        "No Json V1",
                        file.ItemSpec,
                        0,
                        0,
                        0,
                        0,
                        "You shouldn't use the \"CSF Json version 1\". Please port your file to \"version 2\" or use \"Csf Yaml version 1\" to replace that.");
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
                case "csf":
                    Log.LogWarning(
                        "Shimakaze.Sdk.Csf",
                        "CSF0001",
                        "No Json V1",
                        file.ItemSpec,
                        0,
                        0,
                        0,
                        0,
                        "You shouldn't use the \"CSF File\" direct in your project. Please port your file to \"version 2\" or use \"Csf Yaml version 1\" to replace that.");
                    break;
                default:
                    Log.LogError(
                        "Shimakaze.Sdk.Csf",
                        "CSF0002",
                        "Not Support",
                        file.ItemSpec,
                        0,
                        0,
                        0,
                        0,
                        "Unsupport Type: \"{0}\".",
                        tag);
                    return false;
            }
            using ServiceProvider provider = services.BuildServiceProvider();
            if (provider.GetRequiredService<IDeserializer<CsfDocument?>>().Deserialize() is not CsfDocument csf)
            {
                Log.LogError(
                    "Shimakaze.Sdk.Csf",
                    "CSF0003",
                    "Deserialize Failed",
                    file.ItemSpec,
                    0,
                    0,
                    0,
                    0,
                    "Cannot Deserialize the file content to Csf Document.");
                if (File.Exists(dest))
                {
                    output.Dispose();
                    File.Delete(dest);
                }
                return false;
            }
            provider.GetRequiredService<ISerializer<CsfDocument>>().Serialize(csf);
            file.SetMetadata(Metadata_Tag, "Csf");
        }
        OutputFiles = SourceFiles;

        return !Log.HasLoggedErrors;
    }
}
