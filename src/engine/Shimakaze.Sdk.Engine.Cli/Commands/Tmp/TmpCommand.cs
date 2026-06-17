using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Tmp;

[CliCommand(Description = nameof(Resource.Command_Tmp_Description), Parent = typeof(RootCommand))]
internal sealed class TmpCommand
{
}
