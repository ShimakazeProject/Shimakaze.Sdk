﻿using System.Globalization;

using DotMake.CommandLine;

using ShellProgressBar;

using Shimakaze.Sdk.Ini;

namespace Shimakaze.Sdk.Mix.Toolkit;

[CliCommand(
    Description = "将若干文件打包为一个MIX文件。",
    Parent = typeof(RootCommand))]
internal sealed class PackCommand
{
    [CliOption(Description = "将要被打包的文件列表。", Arity = CliArgumentArity.OneOrMore, AllowMultipleArgumentsPerToken = true)]
    public required List<FileInfo> Input { get; set; }

    [CliArgument(Description = "生成的MIX文件的路径。")]
    public required FileInfo Output { get; set; }

    [CliOption(Description = "文件名对照表位置。留空则不生成文件名对照表")]
    public FileInfo? NameMap { get; set; } = default;

    [CliOption(Description = "使用适用于 Red Alert 或 Tiberian Down 的游戏引擎的ID计算器")]
    public bool IsTD { get; set; }

    public async Task RunAsync()
    {
        using StreamWriter? nameMapWriter = NameMap?.CreateText();

        using IndeterminateProgressBar progressBar = new("打包中...", new ProgressBarOptions()
        {
            // TODO: 这里需要注意，编码长度 != 字符宽度
            MessageEncodingName = "GB18030",
        });

        MixBuilder builder;
        using (IndeterminateChildProgressBar initProgressBar = progressBar.SpawnIndeterminate("正在初始化"))
        {
            builder = new(IsTD
                ? IdCalculaters.TDIdCalculater
                : IdCalculaters.TSIdCalculater);

            if (nameMapWriter is not null)
            {
                nameMapWriter.WriteLine("; Generated By Shimakaze.Sdk.Mix.Toolkit");
                nameMapWriter.WriteLine("");
                nameMapWriter.WriteLine("[NameMap]");
                builder.EntryGenerated += (_, e) =>
                {
                    (MixEntry entry, FileInfo file) = e;
                    nameMapWriter.WriteLine($"{entry.Id.ToString("X8", CultureInfo.InvariantCulture)} = {file.Name.ToUpperInvariant()}");
                };
            }

            Input.ForEach(builder.Files.Add);
            initProgressBar.Finished();
        }

        ChildProgressBar? cpb = default;
        builder.WriteEntries += (_, e) =>
        {
            cpb?.Dispose();
            cpb = progressBar.Spawn(Input.Count, "写入文件元数据");
            e.Progress = cpb.AsProgress<int>(
            i => $"正在写入文件元数据: {i}/{Input.Count}",
            i => i / Input.Count
            );
        };
        builder.WritedEntries += (_, _) => cpb?.Dispose();

        builder.WriteFiles += (_, e) =>
        {
            cpb?.Dispose();
            cpb = progressBar.Spawn(Input.Count, "写入文件");
            e.Progress = cpb.AsProgress<int>(
            i => $"正在写入文件: {i}/{Input.Count}",
            i => i / Input.Count
            );
        };
        builder.WritedFiles += (_, _) => cpb?.Dispose();

        using var output = Output.Create();
        await builder.BuildAsync(output);
    }
}
