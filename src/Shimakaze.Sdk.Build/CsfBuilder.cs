using Microsoft.Build.Framework;

using Shimakaze.Sdk.Data.Csf.Serialization;
using Shimakaze.Sdk.Text.Csf.Json.Serialization;
using Shimakaze.Sdk.Text.Csf.Xml.Serialization;
using Shimakaze.Sdk.Text.Csf.Yaml.Serialization;

using MSTask = Microsoft.Build.Utilities.Task;
using TaskItem = Microsoft.Build.Utilities.TaskItem;

namespace Shimakaze.Sdk.Build;

/// <summary>
/// Csf 构建器
/// </summary>
public sealed class CsfBuilder : MSTask
{
    /// <summary>
    /// 将要被处理的文件列表
    /// </summary>
    [Required]
    public required string Files { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    [Required]
    public required string Type { get; set; }

    /// <summary>
    /// 目标目录
    /// </summary>
    [Required]
    public required string TargetDirectory { get; set; }

    /// <summary>
    /// 输出的文件的地址
    /// </summary>
    [Output]
    public ITaskItem[] OutputFiles { get; private set; } = Array.Empty<ITaskItem>();

    /// <inheritdoc/>
    public override bool Execute()
    {
        List<ITaskItem> outputFiles = new();

        switch (Type.ToLowerInvariant())
        {
            case "jsonv1":
                ConvertFromJsonV1(outputFiles);
                break;
            case "json":
            case "jsonv2":
                ConvertFromJsonV2(outputFiles);
                break;
            case "xml":
            case "xmlv1":
                ConvertFromXmlV1(outputFiles);
                break;
            case "yml":
            case "yaml":
            case "ymlv1":
            case "yamlv1":
                ConvertFromYamlV1(outputFiles);
                break;
            default:
                throw new NotSupportedException(Type);
        }

        OutputFiles = outputFiles.ToArray();

        return true;
    }

    private void ConvertFromJsonV1(List<ITaskItem> list)
    {
        foreach (var file in Files.Split(';'))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var target = Path.Combine(TargetDirectory, name + ".csf");
            list.Add(new TaskItem(target));

            using TextReader tr = File.OpenText(file);
            using Stream output = File.Create(target);
            var csf = CsfJsonV1Serializer.Deserialize(tr);
            if (csf is null)
                throw new Exception();

            CsfSerializer.Serialize(output, csf);
        }
    }

    private void ConvertFromJsonV2(List<ITaskItem> list)
    {
        foreach (var file in Files.Split(';'))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var target = Path.Combine(TargetDirectory, name + ".csf");
            list.Add(new TaskItem(target));

            using TextReader tr = File.OpenText(file);
            using Stream output = File.Create(target);
            var csf = CsfJsonV2Serializer.Deserialize(tr);
            if (csf is null)
                throw new Exception();

            CsfSerializer.Serialize(output, csf);
        }
    }

    private void ConvertFromXmlV1(List<ITaskItem> list)
    {
        foreach (var file in Files.Split(';'))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var target = Path.Combine(TargetDirectory, name + ".csf");
            list.Add(new TaskItem(target));

            using TextReader tr = File.OpenText(file);
            using Stream output = File.Create(target);
            var csf = CsfXmlV1Serializer.Deserialize(tr) ?? throw new Exception();
            CsfSerializer.Serialize(output, csf);
        }
    }

    private void ConvertFromYamlV1(List<ITaskItem> list)
    {
        foreach (var file in Files.Split(';'))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var target = Path.Combine(TargetDirectory, name + ".csf");
            list.Add(new TaskItem(target));

            using TextReader tr = File.OpenText(file);
            using Stream output = File.Create(target);
            var csf = CsfYamlV1Serializer.Deserialize(tr);
            if (csf is null)
                throw new Exception();

            CsfSerializer.Serialize(output, csf);
        }
    }

}
