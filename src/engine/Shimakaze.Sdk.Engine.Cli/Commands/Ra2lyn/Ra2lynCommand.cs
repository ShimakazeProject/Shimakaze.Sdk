using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Ra2lyn;

/// <summary>
/// Ra2lyn 子命令根节点：与 INI 编译器相关的操作入口。
/// </summary>
/// <remarks>
/// 名称显式声明为 <c>ra2lyn</c>，避免命令行解析器把 <c>Ra2lyn</c> 拆分成 <c>ra-2-lyn</c>。
/// </remarks>
[CliCommand(Description = nameof(Resource.Command_Ra2lyn_Description), Name = "ra2lyn", Alias = "ini", Parent = typeof(RootCommand))]
internal sealed class Ra2lynCommand
{
}
