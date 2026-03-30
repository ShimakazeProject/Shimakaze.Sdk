using System.Diagnostics.CodeAnalysis;

using DotMake.CommandLine;

using Sharprompt;

using Shimakaze.Sdk.Pal;

using SkiaSharp;

namespace Shimakaze.Sdk.Tmp.Renderer;

[CliCommand(Description = "Shimakaze.Sdk Tmp 渲染器")]
internal sealed class RootCommand
{
    [CliArgument(Description = "模板文件", Required = true)]
    public string? Template { get; set; }

    [CliArgument(Description = "参考的调色板文件", Required = true)]
    public string? Palette { get; set; }

    [CliArgument(Description = "输出的 BMP 文件", Required = true)]
    public string? Output { get; set; }

    [CliOption(Description = "使用安静模式，不提示用户交互")]
    public bool Quiet { get; set; }

    [MemberNotNull(nameof(Template), nameof(Palette), nameof(Output))]
    public void Assert()
    {
        ArgumentNullException.ThrowIfNull(Template);
        ArgumentNullException.ThrowIfNull(Palette);
        ArgumentNullException.ThrowIfNull(Output);
    }

    public void UsePrompt()
    {
        if (Template is null || !File.Exists(Template))
        {
        Template:
            Template = Prompt.Input<string>("请输入图像列表文件路径");
            if (Template is null || !File.Exists(Template))
            {
                Console.Error.WriteLine("无效的图像列表文件路径");
                goto Template;
            }
        }

        if (Palette is null || !File.Exists(Palette))
        {
        Palette:
            Palette = Prompt.Input<string>("请输入参考的调色板文件路径");
            if (Palette is null || !File.Exists(Palette))
            {
                Console.Error.WriteLine("无效的调色板文件路径");
                goto Palette;
            }
        }

        if (Output is null)
        {
        Output:
            Output = Prompt.Input<string>("请输入输出的 SHP(TS) 文件路径");
            if (Output is null)
            {
                Console.Error.WriteLine("无效的 SHP(TS) 文件路径");
                goto Output;
            }
            if (File.Exists(Output)
                && !Prompt.Confirm("已存在同名文件，是否覆盖？", false))
                goto Output;
        }
    }

    public async Task RunAsync()
    {
        if (!Quiet)
            UsePrompt();

        Assert();

        Palette palette;
        await using (var fs = File.OpenRead(Palette))
            palette = Pal.Palette.ReadFrom(fs);

        TemplateFile template;
        await using (var fs = File.OpenRead(Palette))
            template = TemplateFile.ReadFrom(fs);

        int width = (int)((template.Header.BlockWidth + template.Header.BlockHeight) * (template.Header.BlockImageWidth / 2.0));
        int height = (int)((template.Header.BlockWidth + template.Header.BlockHeight) * (template.Header.BlockImageHeight / 2.0));
        using SKBitmap bitmap = new(width, height, true);
        using SKCanvas canvas = new(bitmap);
        // TODO: 待完成绘制
    }
}
