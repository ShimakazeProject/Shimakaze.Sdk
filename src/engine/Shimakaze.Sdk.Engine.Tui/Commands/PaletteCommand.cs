using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Tui.Viewers;
using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Engine.Tui.Commands;

/// <summary>
/// PAL 查看/编辑命令。
/// </summary>
[CliCommand(Description = "Shimakaze.Sdk PAL 查看/编辑器", Alias = "pal", Parent = typeof(RootCommand))]
internal sealed class PaletteCommand
{
    /// <summary>
    /// 调色板文件。
    /// </summary>
    [CliArgument(Description = "调色板文件")]
    public required FileInfo Palette { get; set; }

    /// <summary>
    /// 运行调色板查看器。
    /// </summary>
    public void Run()
    {
        Palette? palette = null;
        if (Palette is not { Exists: true })
            throw new FileNotFoundException();

        using (var fs = Palette.OpenRead())
            palette = Pal.Palette.ReadFrom(fs);

        PaletteViewer viewer = new(palette ?? new(), Palette);
        viewer.Run();
    }
}
