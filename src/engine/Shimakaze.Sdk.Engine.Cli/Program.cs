using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Cli.Commands;
using Shimakaze.Sdk.Engine.Cli.TUI;

TerminalImageSupport.Detect();

CliSettings settings = new()
{
    EnablePosixBundling = true,
    EnableDefaultExceptionHandler = true,
    EnableEnvironmentVariablesDirective = true,
    EnableSuggestDirective = true,
    EnableDiagramDirective = true,
    Theme = CliTheme.Blue,
};

return await Cli.RunAsync<RootCommand>(args, settings);
