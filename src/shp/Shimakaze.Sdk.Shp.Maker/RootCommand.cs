using System.Diagnostics.CodeAnalysis;

using DotMake.CommandLine;

using Sharprompt;

using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Shp.Maker;

[CliCommand(Description = "Shimakaze.Sdk Shp 生成器")]
internal sealed class RootCommand
{
    [CliArgument(Description = "图像列表文件", Required = true)]
    public FileInfo? Input { get; set; }

    [CliArgument(Description = "参考的调色板文件", Required = true)]
    public FileInfo? Palette { get; set; }

    [CliArgument(Description = "输出的 SHP(TS) 文件", Required = true)]
    public FileInfo? Output { get; set; }

    [CliOption(Description = "输出的包含 Sequence 的 INI 文件", Required = false)]
    public FileInfo? SequenceIniOutput { get; set; }

    [CliOption(Description = "调色板结束索引")]
    public int EndIndex { get; set; } = 240;

    [CliOption(Description = "使用安静模式，不提示用户交互")]
    public bool Quiet { get; set; }

    [MemberNotNull(nameof(Input), nameof(Palette), nameof(Output))]
    public void Assert()
    {
        ArgumentNullException.ThrowIfNull(Input);
        ArgumentNullException.ThrowIfNull(Palette);
        ArgumentNullException.ThrowIfNull(Output);
    }

    public void UsePrompt()
    {
        if (Input is null or { Exists: false })
        {
        Input:
            Input = Prompt.Input<FileInfo>("请输入图像列表文件路径");
            if (Input is null or { Exists: false })
            {
                Console.Error.WriteLine("无效的图像列表文件路径");
                goto Input;
            }
        }

        if (Palette is null or { Exists: false })
        {
        Palette:
            Palette = Prompt.Input<FileInfo>("请输入参考的调色板文件路径");
            if (Palette is null or { Exists: false })
            {
                Console.Error.WriteLine("无效的调色板文件路径");
                goto Palette;
            }
        }

        if (Output is null)
        {
        Output:
            Output = Prompt.Input<FileInfo>("请输入输出的 SHP(TS) 文件路径");
            if (Output is null)
            {
                Console.Error.WriteLine("无效的 SHP(TS) 文件路径");
                goto Output;
            }
            if (Output is { Exists: true }
                && !Prompt.Confirm("已存在同名文件，是否覆盖？", false))
                goto Output;
        }

        if (SequenceIniOutput is null)
        {
        SequenceIniOutput:
            SequenceIniOutput = Prompt.Input<FileInfo>("请输入输出的包含 Sequence 的 INI 文件路径");
            if (SequenceIniOutput is { Exists: true }
                && !Prompt.Confirm("已存在同名文件，是否覆盖？", false))
                goto SequenceIniOutput;
        }
    }

    public async Task RunAsync()
    {
        if (!Quiet)
            UsePrompt();

        Assert();

        Palette palette;
        await using (var fs = Palette.OpenRead())
            palette = PaletteReader.Read(fs);

        ShpBuilder builder = new(palette, EndIndex);
        builder.Load(Input.FullName);

        using var writer = SequenceIniOutput?.CreateText() ?? Console.Out;

        await builder.WriteIniAsync(writer);

        await using (FileStream fs = Output.Create())
        {
            List<ShapeImageFrame> frames = [];
            await foreach (var item in builder.BuildAsync())
                frames.Add(item);

            ShapeWriter.Write(fs, new(new()
            {
                Width = (ushort)builder.Width,
                Height = (ushort)builder.Height,
                NumImages = (ushort)frames.Count,
            }, [.. frames]));
            await fs.FlushAsync();
        }
    }
}
