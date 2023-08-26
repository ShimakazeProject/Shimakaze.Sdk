using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Extensions.DependencyInjection;

using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.IO.Csf;
using Shimakaze.Sdk.IO.Csf.Json;
using Shimakaze.Sdk.IO.Csf.Xml;
using Shimakaze.Sdk.IO.Csf.Yaml;

using MSTask = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Build;

/// <summary>
/// Csf 构建器
/// </summary>
public sealed class TaskCsfGenerator : MSTask
{
    /// <summary>
    /// 生成的中间文件的路径
    /// </summary>
    public const string Metadata_Intermediate = "Intermediate";

    /// <summary>
    /// Type
    /// </summary>
    public const string Metadata_Type = "Type";

    /// <summary>
    /// 生成的目标文件
    /// </summary>
    [Output]
    public ITaskItem[] OutputFiles { get; set; } = Array.Empty<ITaskItem>();

    /// <summary>
    /// 将要被处理的文件
    /// </summary>
    [Required]
    public required ITaskItem[] SourceFiles { get; set; }

    /// <inheritdoc />
    public override bool Execute()
    {
        Log.LogMessage("Generating CSF File...");

        ServiceCollection services = new();
        List<ITaskItem> items = new(SourceFiles.Length);
        foreach (var file in SourceFiles)
        {
            var dest = file.GetMetadata(Metadata_Intermediate);
            var tag = file.GetMetadata(Metadata_Type);
            if (!dest.CreateParentDirectory(Log))
                return false;

            services.Clear();
            using Stream stream = File.OpenRead(file.ItemSpec);
            using Stream output = File.Create(dest);
            services.AddSingleton<AsyncWriter<CsfDocument>>(new CsfWriter(output));

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
                    services.AddSingleton<AsyncReader<CsfDocument>>(new CsfJsonV1Reader(stream));
                    break;

                case "json":
                case "jsonv2":
                    services.AddSingleton<AsyncReader<CsfDocument>>(new CsfJsonV2Reader(stream));
                    break;

                case "xml":
                case "xmlv1":
                    services.AddSingleton<AsyncReader<CsfDocument>>(new CsfXmlV1Reader(stream));
                    break;

                case "yml":
                case "yaml":
                case "ymlv1":
                case "yamlv1":
                    services.AddSingleton<AsyncReader<CsfDocument>>(new CsfYamlV1Reader(stream));
                    break;

                case "csf":
                    Log.LogWarning(
                        "Shimakaze.Sdk.Csf",
                        "CSF0001",
                        "No CSF",
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
            CsfDocument csf;
            try
            {
                csf = provider.GetRequiredService<AsyncReader<CsfDocument>>().ReadAsync().Result;
            }
            catch (Exception e)
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
                Log.LogErrorFromException(e);
                if (File.Exists(dest))
                {
                    output.Dispose();
                    File.Delete(dest);
                }
                return false;
            }
            provider.GetRequiredService<AsyncWriter<CsfDocument>>().WriteAsync(csf).Wait();
            TaskItem item = new(dest);
            file.CopyMetadataTo(item);
            item.RemoveMetadata(Metadata_Intermediate);
            item.SetMetadata(Metadata_Type, "Csf");
            items.Add(item);
        }
        OutputFiles = items.ToArray();

        return !Log.HasLoggedErrors;
    }
}