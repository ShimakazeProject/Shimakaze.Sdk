using Microsoft.Build.Framework;

using Shimakaze.Sdk.IO.Ini.Serialization;

using MSTask = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Build;

/// <summary>
/// Ini 合并器
/// </summary>
public sealed class IniMerger : MSTask
{
    /// <summary>
    /// 将要被处理的文件
    /// </summary>
    [Required]
    public required string IniFiles { get; set; }

    /// <summary>
    /// 生成的文件
    /// </summary>
    [Required]
    public required string OutputPath { get; set; }

    /// <inheritdoc/>
    public override bool Execute()
    {
        IO.Ini.IniMerger merger = new();
        foreach (var file in IniFiles.Split(';'))
        {
            using var stream = File.OpenText(file);
            using IniDeserializer deserializer = new(stream);
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
