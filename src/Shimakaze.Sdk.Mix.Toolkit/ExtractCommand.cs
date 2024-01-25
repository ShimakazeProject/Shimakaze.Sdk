using System.Globalization;

using DotMake.CommandLine;

using ShellProgressBar;

using Shimakaze.Sdk.Ini;

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

    public void Run()
    {
        using IndeterminateProgressBar progressBar = new("释放中...", new ProgressBarOptions()
        {
            // TODO: 这里需要注意，编码长度 != 字符宽度
            MessageEncodingName = "GB18030",
        });
        using IndeterminateChildProgressBar initProgressBar = progressBar.SpawnIndeterminate("初始化");

        Output.Create();

        IDictionary<string, string>? nameMap = default;
        if (NameMap is not null)
        {
            using var reader = NameMap.OpenText();
            using IniTokenReader tokens = new(reader);
            using IniDocumentBinder binder = new(tokens);
            var ini = binder.Bind();
            if (ini.TryGetSection("NameMap", out var section))
                nameMap = section;
        }

        nameMap ??= new Dictionary<string, string>();

        using var stream = Input.OpenRead();
        using MixEntryReader entryReader = new(stream);
        var entries = entryReader.ReadAll();
        initProgressBar.Finished();

        using var extractProgressBar = progressBar.Spawn(entries.Length, "释放文件");
        var metadataProgress = extractProgressBar.AsProgress<int>(
            i => $"当前进度 {i}/{entries.Length}",
            i => i / entries.Length
            );

        for (int i = 0; i < entries.Length; i++)
        {
            metadataProgress.Report(i);

            string name = entries[i].Id.ToString("X8", CultureInfo.InvariantCulture);
            if (nameMap.TryGetValue(name, out var value))
                name = value;

            using var pb = extractProgressBar.Spawn(entries[i].Size, $"正在释放 \"{name}\"");
            var progress = pb.AsProgress<int>(
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
