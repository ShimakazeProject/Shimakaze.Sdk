using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Csf.Json;
using Shimakaze.Sdk.Csf.Xml;
using Shimakaze.Sdk.Csf.Yaml;

using MSTask = Microsoft.Build.Utilities.Task;
using Task = System.Threading.Tasks.Task;

namespace Shimakaze.Sdk.Build;

/// <summary>
/// Csf 构建器
/// </summary>
public sealed class TaskCsfGenerator : MSTask
{
    /// <summary>
    /// 生成的中间文件的路径
    /// </summary>
    public const string MetadataIntermediate = "Intermediate";

    /// <summary>
    /// Type
    /// </summary>
    public const string MetadataType = "Type";

    /// <summary>
    /// 生成的目标文件
    /// </summary>
    [Output]
    public ITaskItem[] OutputFiles { get; set; } = [];

    /// <summary>
    /// 将要被处理的文件
    /// </summary>
    [Required]
    public required ITaskItem[] SourceFiles { get; set; }

    /// <inheritdoc />
    public override bool Execute()
    {
        Log.LogMessage("Generating CSF File...");

        List<ITaskItem> items = new(SourceFiles.Length);
        foreach (var file in SourceFiles)
        {
            var dest = file.GetMetadata(MetadataIntermediate);
            var tag = file.GetMetadata(MetadataType);
            if (!dest.CreateParentDirectory(Log))
                return false;

            using Stream stream = File.OpenRead(file.ItemSpec);
            using Stream output = File.Create(dest);

            Task<CsfDocument> reader;
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
                    reader = CsfJsonV1Reader.ReadAsync(stream);
                    break;

                case "json":
                case "jsonv2":
                    reader = CsfJsonV2Reader.ReadAsync(stream);
                    break;

                case "xml":
                case "xmlv1":
                    reader = Task.Run(() =>
                    {
                        using StreamReader sr = new(stream);
                        return CsfXmlV1Reader.Read(sr);
                    });
                    break;

                case "yml":
                case "yaml":
                case "ymlv1":
                case "yamlv1":
                    reader = Task.Run(() =>
                    {
                        using StreamReader sr = new(stream);
                        return CsfYamlV1Reader.Read(sr);
                    });
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
                    reader = Task.Run(() => CsfReader.Read(stream));
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
            CsfDocument csf;
            try
            {
                csf = reader.Result;
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

            CsfWriter.Write(output, csf);
            TaskItem item = new(dest);
            file.CopyMetadataTo(item);
            item.RemoveMetadata(MetadataIntermediate);
            item.SetMetadata(MetadataType, "Csf");
            items.Add(item);
        }
        OutputFiles = [.. items];

        return !Log.HasLoggedErrors;
    }
}