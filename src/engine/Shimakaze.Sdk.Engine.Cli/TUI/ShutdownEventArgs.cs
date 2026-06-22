namespace Shimakaze.Sdk.Engine.Cli.TUI;

internal sealed class ShutdownEventArgs(int exitCode) : EventArgs
{
    public int ExitCode { get; } = exitCode;
}
