using Microsoft.Build.Framework;

using Shimakaze.Sdk.IO.Mix;

using MSTask = Microsoft.Build.Utilities.Task;

namespace Shimakaze.Sdk.Build;

/// <summary>
/// Mix Packer Task
/// </summary>
public sealed class MixPacker : MSTask
{
    /// <summary>
    /// 将要被打包的文件列表
    /// </summary>
    [Required]
    public required string Files { get; set; }

    /// <summary>
    /// 目标文件
    /// </summary>
    [Required]
    public required string TargetFile { get; set; }

    /// <inheritdoc/>
    public override bool Execute()
    {
        var builder = new MixBuilder()
            .SetIdCaculater(IdCalculaters.TSIdCalculater);

        _ = Files
            .Split(';')
            .Select(Path.GetFullPath)
            .Select(i => new FileInfo(i))
            .Select(builder.AddFile)
            .ToList();

        var outdir = Path.GetDirectoryName(TargetFile);
        if (string.IsNullOrEmpty(outdir))
            return false;
        if (!Directory.Exists(outdir))
            Directory.CreateDirectory(outdir);

        using var fs = File.Create(TargetFile);

        builder.BuildAsync(fs).Wait();

        return !Log.HasLoggedErrors;
    }
}
