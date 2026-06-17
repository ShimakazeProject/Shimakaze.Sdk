namespace Shimakaze.Sdk.Engine.Cli.TUI;

internal sealed class ConsoleKeyEventArgs(ConsoleKeyInfo keyInfo) : EventArgs
{
    public ConsoleKeyInfo KeyInfo { get; } = keyInfo;
}
