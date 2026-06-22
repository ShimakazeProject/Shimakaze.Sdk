using System.Drawing;
using System.Text;
using System.Threading.Channels;

using Shimakaze.Sdk.Engine.Cli.TUI.Components;

namespace Shimakaze.Sdk.Engine.Cli.TUI;

internal class NanoFramework : IDisposable
{
    private readonly Channel<EventArgs> _eventChannel = Channel.CreateUnbounded<EventArgs>();
    private readonly StringBuilder _buffer;
    private readonly StringWriter _writer;
    private bool _disposedValue;

    public ShortKeyManager ShortKeyManager { get; }
    public NanoTitleBar TitleBar { get; }
    public NanoHelpBar HelpBar { get; }

    public NanoFramework()
    {
        _buffer = new();
        _writer = new(_buffer);

        ShortKeyManager = new();
        TitleBar = new();
        HelpBar = new(ShortKeyManager);
    }
    public virtual void OnClientRender(TextWriter writer, Size size) { }

    public async Task<int> Run()
    {
        OnInitialize();

        _ = InputLoop();

        int code = 0;
        var updateTask = Task.CompletedTask;
        while (true)
        {
            if (updateTask.IsCompleted)
                updateTask = Task.Run(OnRender);

            var e = await _eventChannel.Reader.ReadAsync();
            OnEvent(e);
            if (e is ShutdownEventArgs shutdown)
            {
                code = shutdown.ExitCode;
                break;
            }

        }

        await updateTask;

        Dispose();

        return code;
    }

    public async void Shutdown(int exit = 0)
    {
        await SendEvent(new ShutdownEventArgs(exit));
    }

    protected virtual void OnInitialize()
    {
        Console.TreatControlCAsInput = true;
        Console.CursorVisible = false;
        Console.Write("\e[?1049h");
    }

    protected virtual void OnRender()
    {
        Size size = new(Console.WindowWidth, Console.WindowHeight);
        Size clientSize = size;
        var titleSize = TitleBar.Measure(clientSize);
        clientSize.Height -= titleSize.Height;
        var helpSize = HelpBar.Measure(clientSize);
        clientSize.Height -= helpSize.Height;

        _buffer.Clear();
        _writer.Write("\e[1;1H");
        TitleBar.OnRender(_writer, titleSize);
        _writer.Write("\e[2;1H");
        OnClientRender(_writer, clientSize);
        _writer.Write($"\e[{size.Height - (helpSize.Height - 1)};1H");
        HelpBar.OnRender(_writer, helpSize);
        _writer.Write("\e[1;1H");
        _writer.Flush();

        Console.WriteLine(_writer);
    }

    protected virtual void OnEvent(EventArgs eventArgs)
    {
        if (eventArgs is ConsoleKeyEventArgs consoleKeyEvent)
            ShortKeyManager.Receive(consoleKeyEvent.KeyInfo);
    }

    protected virtual async ValueTask SendEvent(EventArgs eventArgs)
        => await _eventChannel.Writer.WriteAsync(eventArgs);

    private async Task InputLoop()
    {
        await Task.Yield();
        while (true)
            await SendEvent(new ConsoleKeyEventArgs(Console.ReadKey(true)));
    }

    public static int GetDisplayWidth(ReadOnlySpan<char> text)
    {
        int width = 0;
        bool inEscape = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            // 处理转义序列开始
            if (!inEscape && c == '\x1b')
            {
                inEscape = true;
                continue;
            }

            // 在转义序列中
            if (inEscape)
            {
                if (c == 'm') // 序列结束
                {
                    inEscape = false;
                }
                // 其他字符忽略
                continue;
            }

            // 检查是否为有效的代理对
            if (char.IsHighSurrogate(c))
            {
                int codepoint = char.ConvertToUtf32(c, text[i + 1]);
                width += UnicodeUtils.IsFullWidth(codepoint) ? 2 : 1;
                i++; // 跳过低代理
            }
            else
            {
                // BMP 普通字符（包括 ASCII、非代理的 Unicode 字符）
                // 转换为码点（BMP 字符码点即其值）
                width += UnicodeUtils.IsFullWidth(c) ? 2 : 1;
            }
        }

        return width;
    }


    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            _writer.Dispose();
        }

        Console.Write("\e[?1049l");
        Console.CursorVisible = true;
        _disposedValue = true;
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
