using Shimakaze.Sdk.Engine.Common.Pixels;
using Shimakaze.Sdk.Engine.Tmp;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Tmp;

using XenoAtom.Terminal;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Commands;
using XenoAtom.Terminal.UI.Controls;
using XenoAtom.Terminal.UI.Geometry;
using XenoAtom.Terminal.UI.Graphics;
using XenoAtom.Terminal.UI.Styling;

namespace Shimakaze.Sdk.Engine.Tui.Viewers;

/// <summary>
/// TMP 地形模板查看器。
/// </summary>
internal sealed class TemplateViewer : ViewerBase, IDisposable
{
    private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromMilliseconds(50);

    private readonly TemplateRenderer _renderer;
    private readonly Image _image;
    private readonly SoftwareImageSource _source = new();
    private int _lastColumns;
    private int _lastRows;
    private DateTime _lastPublish;

    /// <summary>
    /// 初始化 <see cref="TemplateViewer"/>。
    /// </summary>
    /// <param name="template">TMP 模板。</param>
    /// <param name="palette">调色板。</param>
    public TemplateViewer(TemplateFile template, Palette palette)
    {
        _renderer = new(template, palette);
        _renderer.Palette[0] = RGBA32.Transparent;

        _image = new()
        {
            ScaleMode = ImageScaleMode.Fit,
            PreserveAspectRatio = true,
            Source = _source,
            ReserveCells = false,
        };
        UpdateImage();
    }

    /// <summary>
    /// 是否必须支持 Kitty/Sixel 图像协议。
    /// </summary>
    protected override bool RequireGraphics => true;

    /// <summary>
    /// 进入全屏前调用，用终端尺寸一次性布置图像。
    /// </summary>
    /// <param name="columns">终端列数。</param>
    /// <param name="rows">终端行数。</param>
    protected override void SetInitialSize(int columns, int rows)
    {
        _lastColumns = columns;
        _lastRows = rows;
        Resize(columns, rows);
    }

    /// <summary>
    /// 每帧更新回调。
    /// </summary>
    public override TerminalLoopResult Update()
    {
        if (DateTime.UtcNow - _lastPublish >= KeepAliveInterval)
        {
            _lastPublish = DateTime.UtcNow;
            _source.Touch();
        }

        var app = RootVisual?.App;
        if (app is not null)
        {
            var size = app.Terminal.Size;
            if (size.Columns != _lastColumns || size.Rows != _lastRows)
            {
                _lastColumns = size.Columns;
                _lastRows = size.Rows;
                Resize(size.Columns, size.Rows);
            }
        }

        return Quit ? TerminalLoopResult.StopAndKeepVisual : TerminalLoopResult.Continue;
    }

    protected override DockLayout Build()
    {
        Header header = new()
        {
            Left = new Markup("[bold]Shimakaze.Sdk TMP Viewer[/]") { Wrap = false },
            Center = new TextBlock("地形模板") { Wrap = false },
            Right = new TextBlock("") { Wrap = false },
        };

        var content = new Border(new Center(_image))
            .Style(BorderStyle.Rounded)
            .Padding(new Thickness(1, 0, 1, 0))
            .HorizontalAlignment(Align.Stretch)
            .VerticalAlignment(Align.Stretch);

        List<Command> commands = new()
        {
            new()
            {
                Id = "tmp.quit",
                LabelMarkup = "退出",
                Gesture = new(TerminalKey.Escape),
                Importance = CommandImportance.Primary,
                Presentation = CommandPresentation.CommandBar,
                Execute = _ => Quit = true,
            },
        };

        var root = new DockLayout
        {
            HorizontalAlignment = Align.Stretch,
            VerticalAlignment = Align.Stretch,
            AutoFocus = true,
        }
            .Top(header)
            .Content(content)
            .Bottom(new ClickableCommandBar(commands));

        foreach (var command in commands)
        {
            root.AddCommand(command);
        }

        return root;
    }

    private void Resize(int columns, int rows)
    {
        _image.CellWidth = Math.Max(10, columns - 4);
        _image.CellHeight = Math.Max(5, rows - 6);
    }

    private void UpdateImage()
    {
        _lastPublish = DateTime.UtcNow;
        var image = _renderer.RenderAsImage().ToSoftware();
        _source.Publish(image);
    }

    /// <inheritdoc />
    public void Dispose() => _source.DisposeAsync().AsTask().GetAwaiter().GetResult();
}
