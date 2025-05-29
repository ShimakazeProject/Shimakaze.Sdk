using System.Globalization;

using DotMake.CommandLine;

using ShellProgressBar;

namespace Shimakaze.Sdk.Mix.Toolkit;

[CliCommand(
    Description = "释放保存在MIX文件中的文件。",
    Parent = typeof(RootCommand))]
internal sealed class ExtractCommand
{
    [CliArgument(Description = "将要被释放的MIX文件")]
    public required FileInfo Input { get; set; }

    [CliArgument(Description = "被释放的文件的存放位置")]
    public required DirectoryInfo Output { get; set; }

    [CliOption(Description = "文件名对照表。根据文件名对照表生成文件名")]
    public FileInfo? NameMap { get; set; } = default;

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

    public void Run()
    {
        using IndeterminateProgressBar progressBar = new("释放中...", new ProgressBarOptions()
        {
            // TODO: 这里需要注意，编码长度 != 字符宽度
            MessageEncodingName = "GB18030",
        });
        using IndeterminateChildProgressBar initProgressBar = progressBar.SpawnIndeterminate("初始化");

        Output.Create();

        var nameMap = LoadNameMap();

        using FileStream stream = Input.OpenRead();
        using MixEntryReader entryReader = new(stream);
        MixEntry[] entries = entryReader.ReadAll();
        initProgressBar.Finished();

        using ChildProgressBar extractProgressBar = progressBar.Spawn(entries.Length, "释放文件");
        IProgress<int> metadataProgress = extractProgressBar.AsProgress<int>(
            i => $"当前进度 {i}/{entries.Length}",
            i => i / entries.Length
            );

        for (int i = 0; i < entries.Length; i++)
        {
            metadataProgress.Report(i);

            string name = entries[i].Id.ToString("X8", CultureInfo.InvariantCulture);
            if (nameMap.TryGetValue(name, out string? value))
            {
                name = value;
            }

            using ChildProgressBar pb = extractProgressBar.Spawn(entries[i].Size, $"正在释放 \"{name}\"");
            IProgress<int> progress = pb.AsProgress<int>(
                i =>
                {
                    string unit = "B";
                    double current = i;
                    double max = entries[i].Size;
                    if (current > 1024)
                    {
                        current /= 1024;
                        max /= 1024;
                        unit = "KB";
                    }
                    if (current > 1024)
                    {
                        current /= 1024;
                        max /= 1024;
                        unit = "MB";
                    }
                    if (current > 1024)
                    {
                        current /= 1024;
                        max /= 1024;
                        unit = "GB";
                    }

                    return $"进度 {current:F2}/{max:F2}({unit})";

                },
                i => i / entries[i].Size
                );

            using Stream output = File.Create(Path.Combine(Output.FullName, name));
            stream.Seek(entryReader.BodyOffset, SeekOrigin.Begin);
            stream.Seek(entries[i].Offset, SeekOrigin.Current);
            for (int j = 0; j < entries[i].Size; j++)
            {
                progress.Report(j);
                output.WriteByte(stream.ReadAsByte());
            }
        }


    }
}
