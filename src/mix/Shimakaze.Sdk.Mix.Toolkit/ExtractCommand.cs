using System.Globalization;

using DotMake.CommandLine;

using ShellProgressBar;

namespace Shimakaze.Sdk.Mix.Toolkit;

[CliCommand(
    Description = "释放保存在MIX文件中的文件。",
    Aliases = ["x"],
    Parent = typeof(RootCommand))]
internal sealed class ExtractCommand
{
    [CliArgument(Description = "将要被释放的MIX文件")]
    public required FileInfo Input { get; set; }

    [CliArgument(Description = "被释放的文件的存放位置")]
    public required DirectoryInfo Output { get; set; }

    [CliOption(Description = "文件名对照表。根据文件名对照表生成文件名")]
    public FileInfo? NameMap { get; set; } = default;

    [CliOption(Description = "适用于 C&C1 / RA1 的无标记 Mix 文件")]
    public bool NoFlag { get; set; }

    private Dictionary<string, string> LoadNameMap()
    {
        Dictionary<string, string> nameMap = [];
        if (NameMap is null)
            return nameMap;

        using StreamReader reader = NameMap.OpenText();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine()!;
            if (line.StartsWith("[NameMap]", StringComparison.Ordinal))
                break;
        }
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine()!;
            if (line.StartsWith('['))
                break;

            var data = line.Split(';', '#')[0];
            var kvp = data.Split('=', StringSplitOptions.TrimEntries);
            nameMap[kvp[0]] = kvp[1];
        }
        return nameMap;
    }

    public async Task RunAsync()
    {
        using IndeterminateProgressBar progressBar = new("释放中...", new ProgressBarOptions()
        {
            // TODO: 这里需要注意，编码长度 != 字符宽度
            MessageEncodingName = "GB18030",
        });
        using IndeterminateChildProgressBar initProgressBar = progressBar.SpawnIndeterminate("初始化");

        Output.Create();

        var nameMap = LoadNameMap();

        await using FileStream stream = Input.OpenRead();
        var entries = Mix.ReadMetadata(stream, out var metadata, out var tag, out var bodyOffset, NoFlag);
        initProgressBar.Finished();

        using ChildProgressBar extractProgressBar = progressBar.Spawn(entries.Length, "释放文件");
        IProgress<int> metadataProgress = extractProgressBar.AsProgress<int>(
            i => $"当前进度 {i}/{entries.Length}",
            i => i / (float)entries.Length
        );

        for (int i = 0; i < entries.Length; i++)
        {
            metadataProgress.Report(i);

            string name = entries[i].Id.ToString("X8", CultureInfo.InvariantCulture);
            if (nameMap.TryGetValue(name, out string? value))
                name = value;

            using ChildProgressBar pb = extractProgressBar.Spawn(entries[i].Size, $"正在释放 \"{name}\"");
            var progress = pb.AsProgress<float>(i => $"进度 {i * 100}%", i => i);

            await using Stream output = File.Create(Path.Combine(Output.FullName, name));
            await Mix.ReadFileAsync(stream, bodyOffset, entries[i], output, progress);
        }
    }
}
