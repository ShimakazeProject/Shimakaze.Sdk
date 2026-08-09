using DotMake.CommandLine;

using Shimakaze.Sdk.Engine.Tui.Commands;

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
