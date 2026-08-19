using Shimakaze.Sdk.Pal;

using XenoAtom.Terminal;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Commands;
using XenoAtom.Terminal.UI.Controls;
using XenoAtom.Terminal.UI.Geometry;
using XenoAtom.Terminal.UI.Input;
using XenoAtom.Terminal.UI.Styling;

namespace Shimakaze.Sdk.Engine.Tui.Viewers;

/// <summary>
/// 调色板查看器/编辑器：16×16 色块网格，支持选择与编辑颜色、保存回文件。
/// </summary>
/// <remarks>
/// 初始化 <see cref="PaletteViewer"/>。
/// </remarks>
/// <param name="file">可选的调色板文件（用于保存）。</param>
/// <param name="palette">调色板。</param>
internal sealed class PaletteViewer(Palette palette, FileInfo? file) : ViewerBase
{
    private readonly State<int> _cursor = new(0);
    private readonly State<int> _version = new(0);
    private readonly State<Color> _editing = new(Color.Rgb(0, 0, 0));
    private bool _editingActive;

    /// <summary>
    /// 每帧更新回调。
    /// </summary>
    public override TerminalLoopResult Update()
    {
        if (_editingActive)
        {
            var editing = _editing.Value;
            PaletteColor pc = new(editing.R, editing.G, editing.B);
            pc.ConvertToPaletteColor();
            if (palette[_cursor.Value] != pc)
            {
                palette[_cursor.Value] = pc;
                _version.Value++;
            }
        }

        return Quit ? TerminalLoopResult.StopAndKeepVisual : TerminalLoopResult.Continue;
    }

    protected override DockLayout Build()
    {
        Canvas? canvas = null;
        canvas = new Canvas()
            .MinHeight(16)
            .MinWidth(48)
            .Painter(ctx =>
            {
                _ = _cursor.Value;
                _ = _version.Value;
                var theme = canvas!.GetTheme();
                var baseStyle = theme.BaseTextStyle();
                int blockWidth = Math.Max(3, ctx.Bounds.Width / 16);

                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        int i = (y << 4) | x;
                        var pc = palette[i];
                        var bg = Color.Rgb(pc.ExpandedR, pc.ExpandedG, pc.ExpandedB);
                        ctx.FillRect(x * blockWidth, y, blockWidth, 1, new(' '), baseStyle.WithBackground(bg));
                    }
                }

                int sy = _cursor.Value >> 4;
                int sx = _cursor.Value & 0xF;
                int bx = sx * blockWidth;
                var sel = palette[_cursor.Value];
                var selBg = Color.Rgb(sel.ExpandedR, sel.ExpandedG, sel.ExpandedB);
                var fg = ContrastForeground(sel);
                ctx.SetPixel(bx + (blockWidth / 2), sy, new('+'), baseStyle.WithBackground(selBg).WithForeground(fg));
            });

        PaletteGrid grid = new(canvas, _cursor, _ => OpenPicker());

        Header header = new()
        {
            Left = new Markup("[bold]Shimakaze.Sdk Palette Viewer[/]") { Wrap = false },
            Center = new TextBlock(() =>
            {
                var pc = palette[_cursor.Value];
                return $"索引 {_cursor.Value}  [{pc.ExpandedR}:{pc.ExpandedG}:{pc.ExpandedB}]";
            })
            { Wrap = false },
            Right = new TextBlock(() => file is null ? "未保存" : file.Name) { Wrap = false },
        };

        var content = new Border(new Center(grid))
            .Style(BorderStyle.Rounded)
            .Padding(new Thickness(1, 1, 1, 1))
            .HorizontalAlignment(Align.Stretch)
            .VerticalAlignment(Align.Stretch);

        List<Command> commands = new()
        {
            new()
            {
                Id = "pal.up",
                LabelMarkup = "上移",
                Gesture = new(TerminalKey.Up),
                Importance = CommandImportance.Secondary,
                Presentation = CommandPresentation.CommandBar,
                Execute = _ => MoveCursor(_cursor.Value - 16),
            },
            new()
            {
                Id = "pal.down",
                LabelMarkup = "下移",
                Gesture = new(TerminalKey.Down),
                Importance = CommandImportance.Secondary,
                Presentation = CommandPresentation.CommandBar,
                Execute = _ => MoveCursor(_cursor.Value + 16),
            },
            new()
            {
                Id = "pal.left",
                LabelMarkup = "左移",
                Gesture = new(TerminalKey.Left),
                Importance = CommandImportance.Secondary,
                Presentation = CommandPresentation.CommandBar,
                Execute = _ => MoveCursor(_cursor.Value - 1),
            },
            new()
            {
                Id = "pal.right",
                LabelMarkup = "右移",
                Gesture = new(TerminalKey.Right),
                Importance = CommandImportance.Secondary,
                Presentation = CommandPresentation.CommandBar,
                Execute = _ => MoveCursor(_cursor.Value + 1),
            },
            new()
            {
                Id = "pal.edit",
                LabelMarkup = "编辑颜色",
                Gesture = new(TerminalKey.Enter),
                Importance = CommandImportance.Primary,
                Presentation = CommandPresentation.CommandBar,
                Execute = _ => OpenPicker(),
            },
            new()
            {
                Id = "pal.save",
                LabelMarkup = "保存",
                Gesture = new('s', TerminalModifiers.None),
                Importance = CommandImportance.Secondary,
                Presentation = CommandPresentation.CommandBar,
                Execute = _ => Save(),
            },
            new()
            {
                Id = "pal.quit",
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

    private void MoveCursor(int index) => _cursor.Value = index & 0xFF;

    private void OpenPicker()
    {
        if (_editingActive)
        {
            return;
        }

        _editingActive = true;
        var original = palette[_cursor.Value];
        _editing.Value = Color.Rgb(original.ExpandedR, original.ExpandedG, original.ExpandedB);

        var picker = new ColorPicker().Value(_editing).HorizontalAlignment(Align.Stretch);
        Dialog? dialog = null;
        dialog = new()
        {
            IsModal = true,
            Width = 48,
            Title = new TextBlock($"编辑颜色 索引 {_cursor.Value}"),
            Content = new VStack(
                picker,
                new HStack(
                    new Button("确定").Tone(ControlTone.Primary).Click(() => ClosePicker(dialog)),
                    new Button("取消").Click(() =>
                    {
                        palette[_cursor.Value] = original;
                        _version.Value++;
                        ClosePicker(dialog);
                    }))
                .Spacing(2)
                .HorizontalAlignment(Align.End))
                .Spacing(1),
        };
        dialog.KeyDown((_, e) =>
        {
            if (e.Key == TerminalKey.Escape)
            {
                palette[_cursor.Value] = original;
                _version.Value++;
                ClosePicker(dialog);
                e.Handled = true;
            }
        });

        dialog.Show();
    }

    private void ClosePicker(Dialog? dialog)
    {
        _editingActive = false;
        dialog?.Close();
    }

    private void Save()
    {
        if (file is null)
        {
            return;
        }

        using var fs = file.OpenWrite();
        palette.WriteTo(fs);
    }

    private static Color ContrastForeground(in PaletteColor color)
    {
        int luminance = (color.ExpandedR * 299 + color.ExpandedG * 587 + color.ExpandedB * 114) / 1000;
        return luminance < 128 ? Color.Rgb(255, 255, 255) : Color.Rgb(0, 0, 0);
    }

    /// <summary>
    /// 包裹色块画布的交互层：鼠标点击选中色块并立即打开取色框。
    /// </summary>
    private sealed class PaletteGrid : ContentVisual
    {
        private readonly State<int> _cursor;
        private readonly Action<int> _onPick;

        public PaletteGrid(Visual content, State<int> cursor, Action<int> onPick)
        {
            Content = content;
            _cursor = cursor;
            _onPick = onPick;
        }

        protected override void OnPointerPressed(PointerEventArgs e)
        {
            if (e.Button != TerminalMouseButton.Left)
            {
                return;
            }

            var rect = Bounds;
            if (rect.Width <= 0 || !rect.Contains(e.UiX, e.UiY))
            {
                return;
            }

            int blockWidth = Math.Max(3, rect.Width / 16);
            int cx = Math.Clamp((e.UiX - rect.X) / blockWidth, 0, 15);
            int cy = Math.Clamp(e.UiY - rect.Y, 0, 15);
            int index = (cy << 4) | cx;
            _cursor.Value = index;
            _onPick(index);
            e.Handled = true;
        }
    }
}
