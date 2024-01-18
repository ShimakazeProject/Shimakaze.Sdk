using DotMake.CommandLine;

namespace Shimakaze.Sdk.Mix.Toolkit;

[CliCommand(Description = "Shimakaze.Sdk Mix工具集")]
internal sealed class RootCommand
{
#pragma warning disable CA1822 // 将成员标记为 static
    public void Run() => Cli.Run<RootCommand>("--help");
#pragma warning restore CA1822 // 将成员标记为 static
}
