using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Tui.Viewers;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Tmp;

namespace Shimakaze.Sdk.Engine.Tui.Commands;

/// <summary>
/// TMP 查看命令。
/// </summary>
[CliCommand(Description = "Shimakaze.Sdk TMP 查看器", Alias = "tmp", Parent = typeof(RootCommand))]
internal sealed class TemplateCommand
{
    /// <summary>
    /// TMP 模板文件。
    /// </summary>
    [CliArgument(Description = "TMP 模板文件")]
    public required FileInfo Template { get; set; }

    /// <summary>
    /// 调色板文件。
    /// </summary>
    [CliOption(Description = "调色板文件", Alias = "p", Aliases = ["pal"])]
    public required FileInfo Palette { get; set; }

    /// <summary>
    /// 运行 TMP 查看器。
    /// </summary>
    public void Run()
    {
        Palette palette;
        using (var fs = Palette.OpenRead())
            palette = Pal.Palette.ReadFrom(fs);

        TemplateFile template;
        using (var fs = Template.OpenRead())
            template = TemplateFile.ReadFrom(fs);

        using TemplateViewer viewer = new(template, palette);
        viewer.Run();
    }
}
