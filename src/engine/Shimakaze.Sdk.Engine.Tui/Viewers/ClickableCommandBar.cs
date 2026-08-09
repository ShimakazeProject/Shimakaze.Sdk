using System.Text;

using XenoAtom.Ansi;
using XenoAtom.Terminal;
using XenoAtom.Terminal.UI;
using XenoAtom.Terminal.UI.Commands;
using XenoAtom.Terminal.UI.Geometry;
using XenoAtom.Terminal.UI.Input;
using XenoAtom.Terminal.UI.Layout;
using XenoAtom.Terminal.UI.Rendering;
using XenoAtom.Terminal.UI.Styling;
using XenoAtom.Terminal.UI.Text;

namespace Shimakaze.Sdk.Engine.Tui.Viewers;

/// <summary>
/// 可点击的命令栏：与框架 <c>CommandBar</c> 一致地展示快捷键指南，并支持鼠标点击直接执行命令。
/// </summary>
internal sealed class ClickableCommandBar : Visual
{
    private readonly IReadOnlyList<Command> _commands;
    private readonly MarkupTextParser _markupParser = new();
    private readonly Dictionary<Command, Rectangle> _bounds = new();
    private readonly State<int> _hoverVersion = new(0);
    private Command? _hoveredCommand;
    private Command? _pressedCommand;

    /// <summary>
    /// 初始化 <see cref="ClickableCommandBar"/>。
    /// </summary>
    /// <param name="commands">需要展示并可点击的命令。</param>
    public ClickableCommandBar(IReadOnlyList<Command> commands)
    {
        _commands = commands;
        HorizontalAlignment = Align.Stretch;
        Focusable = true;
        AutoFocus = true;
    }

    /// <inheritdoc />
    protected override SizeHints MeasureCore(in LayoutConstraints constraints)
    {
        var entries = CollectEntries();
        var markupStyles = GetTheme().GetMarkupStyles();
        var separatorWidth = TerminalTextUtility.GetWidth(GetStyle<CommandBarStyle>().Separator.AsSpan());
        var contentWidth = MeasureContentWidth(entries, markupStyles, separatorWidth);
        var availableWidth = constraints.IsWidthBounded ? Math.Max(1, constraints.MaxWidth) : Math.Max(1, contentWidth);
        var width = Math.Min(contentWidth, availableWidth);
        var height = MeasureWrappedHeight(entries, markupStyles, separatorWidth, availableWidth);
        var natural = constraints.Clamp(new(width, height));
        return SizeHints.Flex(min: new(0, 1), natural: natural, max: natural, growX: 0, growY: 0, shrinkX: 1, shrinkY: 0);
    }

    /// <inheritdoc />
    protected override void RenderOverride(CellBuffer buffer)
    {
        var rect = Bounds;
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }

        _ = _hoverVersion.Value;
        var theme = GetTheme();
        var commandBarStyle = GetStyle<CommandBarStyle>();
        var styles = commandBarStyle.Resolve(theme);
        var markupStyles = theme.GetMarkupStyles();
        var separatorWidth = TerminalTextUtility.GetWidth(commandBarStyle.Separator.AsSpan());

        for (var y = rect.Y; y < rect.Bottom; y++)
        {
            for (var x = rect.X; x < rect.Right; x++)
            {
                buffer.SetCell(x, y, new(' '), styles.BarStyle);
            }
        }

        var entries = CollectEntries();
        _bounds.Clear();
        var row = rect.Y;
        var xCursor = rect.X;
        var hasEntry = false;

        foreach (var cmd in entries)
        {
            var keyText = GetKeyText(in cmd);
            var entryWidth = keyText.Length == 0 ? 0 : MeasureEntryWidth(in cmd, markupStyles);
            if (entryWidth == 0)
            {
                continue;
            }

            if (hasEntry && row < rect.Bottom && xCursor > rect.X && xCursor + separatorWidth + entryWidth > rect.Right)
            {
                row++;
                xCursor = rect.X;
                if (row >= rect.Bottom)
                {
                    break;
                }
            }

            var startX = xCursor;
            if (hasEntry && xCursor > rect.X)
            {
                xCursor = WriteRunes(buffer, rect, xCursor, row, commandBarStyle.Separator, styles.LabelStyle);
            }

            var hovered = ReferenceEquals(_hoveredCommand, cmd);
            xCursor = WriteKeycap(buffer, rect, xCursor, row, keyText, styles.KeyStyle, commandBarStyle);
            xCursor = WriteRunes(buffer, rect, xCursor, row, " ", styles.LabelStyle);
            xCursor = WriteMarkup(buffer, rect, xCursor, row, cmd.LabelMarkup, hovered ? styles.KeyStyle : styles.LabelStyle, markupStyles);

            _bounds[cmd] = new(startX, row, Math.Max(1, xCursor - startX), 1);
            hasEntry = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var command = HitTestCommand(e.UiX, e.UiY);
        if (!ReferenceEquals(_hoveredCommand, command))
        {
            _hoveredCommand = command;
            _hoverVersion.Value++;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerEventArgs e)
    {
        if (e.Button != TerminalMouseButton.Left)
        {
            return;
        }

        _pressedCommand = HitTestCommand(e.UiX, e.UiY);
        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerEventArgs e)
    {
        if (e.Button != TerminalMouseButton.Left)
        {
            return;
        }

        var command = _pressedCommand;
        _pressedCommand = null;
        if (command is not null && ReferenceEquals(HitTestCommand(e.UiX, e.UiY), command))
        {
            command.Execute(this);
        }

        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnHoveredChanged(bool value)
    {
        if (!value)
        {
            _hoveredCommand = null;
            _hoverVersion.Value++;
        }
    }

    private List<Command> CollectEntries()
    {
        List<Command> result = new();
        foreach (var command in _commands)
        {
            if ((command.Presentation & CommandPresentation.CommandBar) == 0)
            {
                continue;
            }

            if (command.Gesture is null && command.Sequence is null)
            {
                continue;
            }

            result.Add(command);
        }

        result.Sort(static (a, b) => a.Importance.CompareTo(b.Importance));
        return result;
    }

    private int MeasureContentWidth(List<Command> entries, Dictionary<string, AnsiStyle> markupStyles, int separatorWidth)
    {
        var width = 0;
        var hasEntry = false;
        foreach (var command in entries)
        {
            var entryWidth = MeasureEntryWidth(in command, markupStyles);
            if (entryWidth == 0)
            {
                continue;
            }

            if (hasEntry)
            {
                width += separatorWidth;
            }

            width += entryWidth;
            hasEntry = true;
        }

        return width;
    }

    private int MeasureWrappedHeight(List<Command> entries, Dictionary<string, AnsiStyle> markupStyles, int separatorWidth, int availableWidth)
    {
        var lineCount = 1;
        var x = 0;
        var hasEntry = false;
        foreach (var command in entries)
        {
            var entryWidth = MeasureEntryWidth(in command, markupStyles);
            if (entryWidth == 0)
            {
                continue;
            }

            var leadingWidth = hasEntry ? separatorWidth : 0;
            if (x > 0 && x + leadingWidth + entryWidth > availableWidth)
            {
                lineCount++;
                x = 0;
                leadingWidth = 0;
            }

            x += leadingWidth + entryWidth;
            hasEntry = true;
        }

        return Math.Max(1, lineCount);
    }

    private int MeasureEntryWidth(in Command command, Dictionary<string, AnsiStyle> markupStyles)
    {
        var keyText = GetKeyText(in command);
        if (keyText.Length == 0)
        {
            return 0;
        }

        var plain = _markupParser.Parse(command.LabelMarkup, out _, markupStyles);
        return 1 + TerminalTextUtility.GetWidth(keyText.AsSpan()) + 1 + 1 + TerminalTextUtility.GetWidth(plain.AsSpan());
    }

    private Command? HitTestCommand(int x, int y)
    {
        foreach (var pair in _bounds)
        {
            if (pair.Value.Contains(x, y))
            {
                return pair.Key;
            }
        }

        return null;
    }

    private static string GetKeyText(in Command command)
        => command.Sequence?.ToString() ?? command.Gesture?.ToString() ?? string.Empty;

    private static int WriteKeycap(CellBuffer buffer, Rectangle rect, int x, int y, ReadOnlySpan<char> keyText, Style style, CommandBarStyle commandBarStyle)
    {
        if (y >= rect.Bottom || x >= rect.Right)
        {
            return x;
        }

        buffer.SetCell(x++, y, commandBarStyle.KeycapOpen, style);
        x = WriteRunes(buffer, rect, x, y, keyText, style);
        if (x < rect.Right)
        {
            buffer.SetCell(x++, y, commandBarStyle.KeycapClose, style);
        }

        return x;
    }

    private static int WriteRunes(CellBuffer buffer, Rectangle rect, int x, int y, ReadOnlySpan<char> text, Style style)
    {
        if (y >= rect.Bottom || x >= rect.Right)
        {
            return x;
        }

        var max = rect.Right;
        foreach (var rune in text.EnumerateRunes())
        {
            if (x >= max)
            {
                break;
            }

            buffer.SetCell(x, y, rune, style);
            x += TerminalTextUtility.GetRuneWidth(rune);
        }

        return x;
    }

    private int WriteMarkup(CellBuffer buffer, Rectangle rect, int x, int y, string labelMarkup, Style baseStyle, Dictionary<string, AnsiStyle> markupStyles)
    {
        if (y >= rect.Bottom || x >= rect.Right)
        {
            return x;
        }

        var plain = _markupParser.Parse(labelMarkup, out var runs, markupStyles);
        if (plain.Length == 0)
        {
            return x;
        }

        for (var i = 0; i < runs.Length && x < rect.Right; i++)
        {
            var run = runs[i];
            var start = run.Start;
            var end = Math.Min(run.Start + run.Length, plain.Length);
            if (end <= start)
            {
                continue;
            }

            x = WriteRunes(buffer, rect, x, y, plain.AsSpan(start, end - start), baseStyle | run.Style);
        }

        if (runs.Length == 0)
        {
            x = WriteRunes(buffer, rect, x, y, plain, baseStyle);
        }

        return x;
    }
}
