namespace Shimakaze.Sdk.Engine.Cli.TUI;

internal class ConsoleKeyEventArgs(ConsoleKeyInfo keyInfo) : EventArgs
{
    public ConsoleKeyInfo KeyInfo { get; } = keyInfo;
}
