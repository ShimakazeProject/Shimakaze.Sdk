using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Tui.Viewers;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Shp;

namespace Shimakaze.Sdk.Engine.Tui.Commands;

/// <summary>
/// SHP 查看命令。
/// </summary>
[CliCommand(Description = "Shimakaze.Sdk SHP 查看器", Alias = "shp", Parent = typeof(RootCommand))]
internal sealed class ShapeCommand
{
    /// <summary>
    /// SHP(TS) 文件。
    /// </summary>
    [CliArgument(Description = "SHP(TS) 文件")]
    public required FileInfo Shape { get; set; }

    /// <summary>
    /// 调色板文件。
    /// </summary>
    [CliOption(Description = "调色板文件", Alias = "p", Aliases = ["pal"])]
    public required FileInfo Palette { get; set; }

    /// <summary>
    /// 运行 SHP 查看器。
    /// </summary>
    public void Run()
    {
        Palette palette;
        using (var fs = Palette.OpenRead())
            palette = Pal.Palette.ReadFrom(fs);

        ShapeImage shp;
        using (var fs = Shape.OpenRead())
            shp = ShapeImage.ReadFrom(fs);

        using ShapeViewer viewer = new(shp, palette);
        viewer.Run();
    }
}
