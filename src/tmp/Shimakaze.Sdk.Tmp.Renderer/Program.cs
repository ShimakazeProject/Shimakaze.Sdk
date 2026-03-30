using DotMake.CommandLine;

using Shimakaze.Sdk.Tmp.Renderer;

if (args is { Length: not 0 })
{
    return Cli.Run<RootCommand>(args);
}
else
{
    RootCommand cmd = new();
    await cmd.RunAsync();
    return 0;
}
