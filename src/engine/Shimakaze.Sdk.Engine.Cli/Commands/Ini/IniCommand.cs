using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Resources;

namespace Shimakaze.Sdk.Engine.Cli.Commands.Ini;

[CliCommand(Description = nameof(Resource.Command_Ini_Description), Parent = typeof(RootCommand))]
internal sealed class IniCommand
{
}
