using DotMake.CommandLine;

using Shimakaze.Sdk.Csf.Converter;

return args switch
{
    { Length: not 0 } => Cli.Run<RootCommand>(args),
    _ => Cli.Run<RootCommand>("--help")
};