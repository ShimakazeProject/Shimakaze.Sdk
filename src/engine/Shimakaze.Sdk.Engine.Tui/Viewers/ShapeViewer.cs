using Shimakaze.Sdk.Engine.Common.Pixels;
using Shimakaze.Sdk.Engine.Shp;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Shp;

using XenoAtom.Terminal;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Commands;
using XenoAtom.Terminal.UI.Controls;
using XenoAtom.Terminal.UI.Geometry;
using XenoAtom.Terminal.UI.Graphics;
using XenoAtom.Terminal.UI.Input;
using XenoAtom.Terminal.UI.Styling;

namespace Shimakaze.Sdk.Engine.Tui.Viewers;

/// <summary>
/// SHP 帧查看器：用 sixel/kitty 显示帧，支持播放、逐帧浏览、阴影切换与修改所属色（键盘 + 鼠标）。
/// </summary>
internal sealed class ShapeViewer : ViewerBase, IDisposable
{
    private const double FrameInterval = 1.0 / 30.0;
    private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromMilliseconds(50);

    private readonly ShapeRenderer _renderer;
    private readonly Image _image;
    private readonly SoftwareImageSource _source = new();
    private readonly State<int> _frame = new(0);
    private readonly State<bool> _playing = new(false);
    private readonly State<bool> _shadow;
    private DateTime _lastFrameTime = DateTime.UtcNow;
    private DateTime _lastPublish;
    private int _lastColumns;
    private int _lastRows;

    /// <summary>
    /// 初始化 <see cref="ShapeViewer"/>。
    /// </summary>
    /// <param name="shp">SHP 图像。</param>
    /// <param name="palette">调色板。</param>
    public ShapeViewer(ShapeImage shp, Palette palette)
    {
        _renderer = new(shp, palette);
        _shadow = new(_renderer.HasShadow);
        _renderer.Palette[0] = RGBA32.Transparent;

        _image = new()
        {
            ScaleMode = ImageScaleMode.Fit,
            PreserveAspectRatio = true,
            Source = _source,
            ReserveCells = false,
        };
        UpdateFrame();
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

        if (_playing.Value && DateTime.UtcNow - _lastFrameTime >= TimeSpan.FromSeconds(FrameInterval))
        {
            _lastFrameTime = DateTime.UtcNow;
            NextFrame();
        }

        return Quit ? TerminalLoopResult.StopAndKeepVisual : TerminalLoopResult.Continue;
    }

    protected override DockLayout Build()
    {
        Header header = new()
        {
            Left = new Markup("[bold]Shimakaze.Sdk SHP Viewer[/]") { Wrap = false },
            Center = new TextBlock(() => $"帧 {_frame.Value + 1} / {_renderer.Count}  {(_playing.Value ? "▶ 播放" : "⏸ 暂停")}") { Wrap = false },
            Right = new TextBlock(() => $"阴影 {(_shadow.Value ? "开" : "关")}") { Wrap = false },
        };

        var content = new Border(new Center(_image))
            .Style(BorderStyle.Rounded)
            .Padding(new Thickness(1, 0, 1, 0))
            .HorizontalAlignment(Align.Center)
            .VerticalAlignment(Align.Center);

        List<Command> commands = new()
        {
            new()
            {
                Id = "shp.togglePlay",
                LabelMarkup = "播放/暂停",
                Gesture = new(TerminalKey.Space),
                Importance = CommandImportance.Primary,
                Presentation = CommandPresentation.CommandBar,
                Execute = _ => TogglePlay(),
            },
            new()
            {
                Id = "shp.prev",
                LabelMarkup = "上一帧",
                Gesture = new(TerminalKey.Left),
                Importance = CommandImportance.Secondary,
                Presentation = CommandPresentation.CommandBar,
                Execute = _ => PrevFrame(),
            },
            new()
            {
                Id = "shp.next",
                LabelMarkup = "下一帧",
                Gesture = new(TerminalKey.Right),
                Importance = CommandImportance.Secondary,
                Presentation = CommandPresentation.CommandBar,
                Execute = _ => NextFrame(),
            },
            new()
            {
                Id = "shp.first",
                LabelMarkup = "第一帧",
                Gesture = new(TerminalKey.Home),
                Importance = CommandImportance.Tertiary,
                Presentation = CommandPresentation.CommandBar,
                Execute = _ => SetFrame(0),
            },
            new()
            {
                Id = "shp.last",
                LabelMarkup = "最后一帧",
                Gesture = new(TerminalKey.End),
                Importance = CommandImportance.Tertiary,
                Presentation = CommandPresentation.CommandBar,
                Execute = _ => SetFrame(_renderer.Count - 1),
            },
            new()
            {
                Id = "shp.shadow",
                LabelMarkup = "阴影",
                Gesture = new('s', TerminalModifiers.None),
                Importance = CommandImportance.Secondary,
                Presentation = CommandPresentation.CommandBar,
                Execute = _ => ToggleShadow(),
            },
            new()
            {
                Id = "shp.houseColor",
                LabelMarkup = "所属色",
                Gesture = new('c', TerminalModifiers.None),
                Importance = CommandImportance.Secondary,
                Presentation = CommandPresentation.CommandBar,
                Execute = _ => ChangeHouseColor(),
            },
            new()
            {
                Id = "shp.quit",
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

    private void TogglePlay()
    {
        _playing.Value = !_playing.Value;
        _lastFrameTime = DateTime.UtcNow;
    }

    private void PrevFrame() => SetFrame(_frame.Value - 1);

    private void NextFrame() => SetFrame(_frame.Value + 1);

    private void SetFrame(int index)
    {
        _frame.Value = index < 0 ? _renderer.Count - 1 : index % _renderer.Count;
        UpdateFrame();
    }

    private void ToggleShadow()
    {
        _renderer.HasShadow = !_renderer.HasShadow;
        _shadow.Value = _renderer.HasShadow;
        UpdateFrame();
    }

    private void ChangeHouseColor()
    {
        var current = _renderer.Palette[16];
        State<Color> editing = new(Color.Rgb(current.R, current.G, current.B));
        var picker = new ColorPicker()
            .Value(editing)
            .Palette([
                Color.Rgb(255, 150, 0), // LightGold
                Color.Rgb(236, 239, 0), // Gold
                Color.Rgb(240, 240, 240), // LightGrey
                Color.Rgb(131, 131, 131), // Grey
                Color.Rgb(184, 87, 0), // Red
                Color.Rgb(255, 25, 25), // DarkRed
                Color.Rgb(255, 160, 25), // Orange
                Color.Rgb(255, 153, 235), // Magenta
                Color.Rgb(149, 40, 189), // Purple
                Color.Rgb(112, 255, 226), // LightBlue
                Color.Rgb(34, 105, 212), // DarkBlue
                Color.Rgb(144, 92, 238), // NeonBlue
                Color.Rgb(50, 212, 230), // DarkSky
                Color.Rgb(11, 195, 93), // Green
                Color.Rgb(61, 210, 45), // DarkGreen
                Color.Rgb(0, 0, 0), // NeonGreen
                Color.Rgb(255, 238, 96), // Yellow
                Color.Rgb(184, 40, 189), // Purple2
                Color.Rgb(114, 40, 189), // Purple3
            ])
            .HorizontalAlignment(Align.Stretch);
        Dialog? dialog = null;
        dialog = new()
        {
            IsModal = true,
            Width = 48,
            Title = new TextBlock("所属色（色板 16-31）"),
            Content = new VStack(
                picker,
                new HStack(
                    new Button("确定").Tone(ControlTone.Primary).Click(() =>
                    {
                        var color = editing.Value;
                        _renderer.UpdateHouseColor(new(color.R, color.G, color.B));
                        UpdateFrame();
                        CloseHousePicker(dialog);
                    }),
                    new Button("取消").Click(() => CloseHousePicker(dialog)))
                .Spacing(2)
                .HorizontalAlignment(Align.End))
                .Spacing(1),
        };
        dialog.KeyDown((_, e) =>
        {
            if (e.Key == TerminalKey.Escape)
            {
                CloseHousePicker(dialog);
                e.Handled = true;
            }
        });
        _image.IsVisible = false;
        dialog.Show();
    }

    private void CloseHousePicker(Dialog? dialog)
    {
        _image.IsVisible = true;
        dialog?.Close();
    }

    private void UpdateFrame()
    {
        _lastPublish = DateTime.UtcNow;
        var image = _renderer.GetFrame(_frame.Value).RenderAsImage().ToSoftware();
        _source.Publish(image);
    }

    /// <inheritdoc />
    public void Dispose() => _source.DisposeAsync().AsTask().GetAwaiter().GetResult();
}
