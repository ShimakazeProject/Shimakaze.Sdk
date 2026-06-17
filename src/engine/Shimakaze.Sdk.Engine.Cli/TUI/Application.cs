using System.Threading.Channels;

namespace Shimakaze.Sdk.Engine.Cli.TUI;

internal abstract class Application : IDisposable
{
    private readonly Channel<EventArgs> _eventChannel = Channel.CreateUnbounded<EventArgs>();

    private async Task InputLoop(CancellationToken cancellationToken)
    {
        await Task.Yield();
        while (true)
            await SendEvent(new ConsoleKeyEventArgs(Console.ReadKey(true)), cancellationToken);
    }

    protected virtual async ValueTask SendEvent(EventArgs eventArgs, CancellationToken cancellationToken)
        => await _eventChannel.Writer.WriteAsync(eventArgs, cancellationToken);

    protected virtual void OnEvent(EventArgs eventArgs)
    {
    }


    public async Task Run(CancellationToken cancellationToken = default)
    {
        _ = InputLoop(cancellationToken);

        Task updateTask = Task.CompletedTask;
        Console.Write("\e[?1049h");
        Console.CursorVisible = false;
        while (true)
        {
            if (updateTask.IsCompleted)
                updateTask = Task.Run(Update, cancellationToken);

            var e = await _eventChannel.Reader.ReadAsync(cancellationToken);
            OnEvent(e);
        }
    }

    protected virtual void Update()
    {
    }

    protected virtual void Dispose(bool disposing)
    {
        Console.Write("\e[?1049l");
    }

    // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
    // ~Application()
    // {
    //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
