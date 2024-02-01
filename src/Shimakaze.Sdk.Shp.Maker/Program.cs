
using DotMake.CommandLine;

using Shimakaze.Sdk.Shp.Maker;

if(args is { Length: not 0 })
{
    return Cli.Run<RootCommand>(args);
}
else
{
    RootCommand cmd = new();
    cmd.UsePrompt();
    await cmd.RunAsync();
    return 0;
}