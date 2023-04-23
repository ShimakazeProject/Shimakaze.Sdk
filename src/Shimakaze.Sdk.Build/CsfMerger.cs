using Microsoft.Build.Framework;

using Shimakaze.Sdk.IO.Csf.Serialization;

using MSTask = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Build;

/// <summary>
/// Csf 合并器
/// </summary>
public sealed class CsfMerger : MSTask
{
    /// <summary>
    /// 将要被处理的文件
    /// </summary>
    [Required]
    public required string CsfFiles { get; set; }

    /// <summary>
    /// 生成的文件
    /// </summary>
    [Required]
    public required string OutputPath { get; set; }

    /// <inheritdoc/>
    public override bool Execute()
    {
        IO.Csf.CsfMerger merger = new();
        foreach (var file in CsfFiles.Split(';'))
        {
            using Stream stream = File.OpenRead(file);
            using CsfDeserializer deserializer = new(stream);
            merger.UnionWith(deserializer.Deserialize());
        }

        var outdir = Path.GetDirectoryName(OutputPath);
        if (string.IsNullOrEmpty(outdir))
            return false;
        if (!Directory.Exists(outdir))
            Directory.CreateDirectory(outdir);
        using Stream output = File.Create(OutputPath);
        merger.BuildAndWriteTo(output);

        return true;
    }
}
