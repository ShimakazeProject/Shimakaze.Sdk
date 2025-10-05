using System.Diagnostics.CodeAnalysis;

using DotMake.CommandLine;

using Sharprompt;

using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Shp.Maker;

[CliCommand(Description = "Shimakaze.Sdk Shp 生成器")]
internal sealed class RootCommand
{
    [CliArgument(Description = "图像列表文件", Required = true)]
    public string? Input { get; set; }

    [CliArgument(Description = "参考的调色板文件", Required = true)]
    public string? Palette { get; set; }

    [CliArgument(Description = "输出的 SHP(TS) 文件", Required = true)]
    public string? Output { get; set; }

    [CliOption(Description = "输出的包含 Sequence 的 INI 文件", Required = false)]
    public string? SequenceIniOutput { get; set; }

    [CliOption(Description = "调色板开始索引")]
    public int StartIndex { get; set; } = -1;

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
        if (Input is null || !File.Exists(Input))
        {
        Input:
            Input = Prompt.Input<string>("请输入图像列表文件路径");
            if (Input is null || !File.Exists(Input))
            {
                Console.Error.WriteLine("无效的图像列表文件路径");
                goto Input;
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

        if (SequenceIniOutput is null)
        {
        SequenceIniOutput:
            SequenceIniOutput = Prompt.Input<string>("请输入输出的包含 Sequence 的 INI 文件路径");
            if (File.Exists(SequenceIniOutput)
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
        await using (var fs = File.OpenRead(Palette))
            palette = Pal.Palette.ReadFrom(fs);

        ShpBuilder builder = new(palette, StartIndex, EndIndex);
        builder.Load(Path.GetFullPath(Input));

        using var writer = SequenceIniOutput is null ? Console.Out : File.CreateText(SequenceIniOutput);

        await builder.WriteIniAsync(writer);

        await using (var fs = File.Create(Output))
        {
            List<ShapeImageFrame> frames = [];
            await foreach (var item in builder.BuildAsync())
                frames.Add(item);

            new ShapeImage(new()
            {
                Width = (ushort)builder.Width,
                Height = (ushort)builder.Height,
                NumImages = (ushort)frames.Count,
            }, [.. frames]).WriteTo(fs);

            await fs.FlushAsync();
        }
    }
}
