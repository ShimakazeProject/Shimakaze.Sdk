using XenoAtom.Terminal;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Graphics;

namespace Shimakaze.Sdk.Engine.Tui.Viewers;

/// <summary>
/// 全屏 TUI 查看器基类。
/// </summary>
internal abstract class ViewerBase
{
    private Visual? _root;
    private bool _quit;

    /// <summary>
    /// 根视觉树。
    /// </summary>
    public Visual Root => _root ??= Build();

    /// <summary>
    /// 以全屏方式运行查看器。
    /// </summary>
    public void Run()
    {
        using var session = Terminal.Open();

        if (RequireGraphics && !Terminal.Graphics.Capabilities.SupportsStaticImages)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine("错误：当前终端不支持 Kitty 或 Sixel 图像协议，无法显示图像。");
            Console.Error.WriteLine("请改用支持 Kitty/Sixel 的终端（如 WezTerm、Ghostty、Windows Terminal Preview、Tabby 等）运行本命令。");
            Console.Error.WriteLine();
            return;
        }

        SetInitialSize(Terminal.Size.Columns, Terminal.Size.Rows);

        TerminalRunOptions options = new()
        {
            GraphicsPresenter = new TerminalImageGraphicsPresenter(new()),
        };
        using var app = Terminal.Run(
            Root,
            _ => Update(),
            options);
    }

    /// <summary>
    /// 每帧更新回调。
    /// </summary>
    public abstract TerminalLoopResult Update();

    /// <summary>
    /// 构建根视觉树。
    /// </summary>
    protected abstract Visual Build();

    /// <summary>
    /// 已构建的根视觉树。
    /// </summary>
    protected Visual? RootVisual => _root;

    /// <summary>
    /// 退出标志。
    /// </summary>
    protected bool Quit
    {
        get => _quit;
        set => _quit = value;
    }

    /// <summary>
    /// 是否必须支持 Kitty/Sixel 图像协议。
    /// </summary>
    protected virtual bool RequireGraphics => false;

    /// <summary>
    /// 进入全屏前调用，用终端尺寸布置内容。
    /// </summary>
    /// <param name="columns">终端列数。</param>
    /// <param name="rows">终端行数。</param>
    protected virtual void SetInitialSize(int columns, int rows)
    {
    }
}
