using System.Globalization;
using System.Text;
using System.Timers;

using Shimakaze.Sdk.Engine.Cli.Components;
using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Common.Pixels;
using Shimakaze.Sdk.Engine.Shp;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Shp;

using Timer = System.Timers.Timer;

namespace Shimakaze.Sdk.Engine.Cli.TUI.App;

internal enum ShpViewerMode
{
    Normal,
    HouseColorPicker
}

internal sealed class ShpViewer : Application
{
    private readonly BGRA32 _transparentShadowColor = new(56, 56, 56);
    private readonly BGRA32 _shadowColor;
    private readonly CompositeFormat _hueFormat = CompositeFormat.Parse(Resource.TUI_ShpViewer_HueFormat);
    private readonly StringBuilder _buffer;
    private readonly StringWriter _writer;
    private readonly ShpRenderer _renderer;
    private readonly SixelImage _sixel = new() { Center = true };
    private readonly Timer _timer;
    private bool _play;
    private ShpViewerMode _mode;
    private BGRA32 _latestHouseColor;
    private BGRA32 _currentHouseColor;
    private int _h;
    private int _index;
    private bool _disposedValue;

    public ShpViewer(ShapeImage shp, Palette pal)
    {
        _buffer = new();
        _writer = new(_buffer);

        _timer = new(TimeSpan.FromSeconds(1 / 30d));
        _timer.Elapsed += TimerElapsed;
        _renderer = new(shp, pal)
        {
            UseTransparent = false,
        };
        _shadowColor = _renderer.Palette[1];
    }

    private async void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        await SendEvent(e, default);
    }

    protected override void OnEvent(EventArgs eventArgs)
    {
        switch (eventArgs)
        {
            case ConsoleKeyEventArgs keyEventArgs:
                OnKeyEvent(keyEventArgs.KeyInfo);
                break;
            case ElapsedEventArgs:
                _index++;
                if (_index >= _renderer.Count)
                    _index = 0;
                break;
            default:
                break;
        }

    }

    private void WriteHelp(TextWriter writer)
    {
        if (_mode is ShpViewerMode.Normal)
        {
            string l1 = $"{(_play ? "▶" : "⏸")}  \e[32m{_index + 1}\e[0m / \e[32m{_renderer.Count}\e[0m";
            int length = l1.Length - 16;
            int padding = (Console.WindowWidth - length) / 2;
            writer.Write($"\e[2K");
            writer.Write($"\e[{padding}C");
            writer.WriteLine(l1);

            string[] fields =
            [
                _play ? $"\e[92m\e[47m Space \e[0m {Resource.TUI_ShpViewer_Pause}" : $"\e[30m\e[47m Space \e[0m {Resource.TUI_ShpViewer_Play}",
                $"\e[30m\e[47m H \e[0m {Resource.TUI_ShpViewer_ToggleHouse}",
                $"\e[30m\e[47m ← \e[0m {Resource.TUI_ShpViewer_PrevFrame}",
                $"\e[30m\e[47m → \e[0m {Resource.TUI_ShpViewer_NextFrame}",
                _renderer.HasShadow ? $"\e[92m\e[47m S \e[0m {Resource.TUI_ShpViewer_DisableShadow}" : $"\e[30m\e[47m S \e[0m {Resource.TUI_ShpViewer_EnableShadow}",
                _renderer.UseTransparent ? $"\e[92m\e[47m T \e[0m {Resource.TUI_ShpViewer_DisableTransparent}" : $"\e[30m\e[47m T \e[0m {Resource.TUI_ShpViewer_EnableTransparent}",
            ];

            int t = Console.WindowWidth / fields.Length;
            for (int i = 0; i < fields.Length; i++)
            {
                writer.Write($"\e[{t * i}G");
                writer.Write(fields[i]);
            }
        }
        else if (_mode is ShpViewerMode.HouseColorPicker)
        {
            var c = _currentHouseColor;
            string l1 = $"{string.Format(CultureInfo.InvariantCulture, _hueFormat, _h)} \e[48;2;{c.R};{c.G};{c.B}m     \e[0m";
            int length = l1.Length - 16;
            int padding = (Console.WindowWidth - length) / 2;
            writer.Write($"\e[2K");
            writer.Write($"\e[{padding}C");
            writer.WriteLine(l1);

            string[] fields =
            [
                $"\e[30m\e[47m Escape \e[0m {Resource.TUI_ShpViewer_Exit}",
                $"\e[30m\e[47m Enter \e[0m {Resource.TUI_ShpViewer_SelectColor}",
                $"\e[30m\e[47m ↑ \e[0m {Resource.TUI_ShpViewer_HueIncrease}",
                $"\e[30m\e[47m ↓ \e[0m {Resource.TUI_ShpViewer_HueDecrease}",
                _renderer.HasShadow ? $"\e[92m\e[47m S \e[0m {Resource.TUI_ShpViewer_DisableShadow}" : $"\e[30m\e[47m S \e[0m {Resource.TUI_ShpViewer_EnableShadow}",
                _renderer.UseTransparent ? $"\e[92m\e[47m T \e[0m {Resource.TUI_ShpViewer_DisableTransparent}" : $"\e[30m\e[47m T \e[0m {Resource.TUI_ShpViewer_EnableTransparent}",
            ];

            int t = Console.WindowWidth / fields.Length;
            for (int i = 0; i < fields.Length; i++)
            {
                writer.Write($"\e[{t * i}G");
                writer.Write(fields[i]);
            }
        }
    }

    protected override void Update()
    {
        _sixel.SetImage(_renderer.GetFrame(_index).RenderAsImage());

        int i = Console.WindowHeight - 1;
        _buffer.Clear();
        if (_renderer.UseTransparent)
            _writer.Write("\e[2J");
        _writer.Write("\e[1;1H");
        _writer.Write(_sixel);
        _writer.Write($"\e[{i};1H");
        WriteHelp(_writer);

        Console.Write(_writer);
    }

    private void OnKeyEvent(ConsoleKeyInfo key)
    {
        if (_mode is ShpViewerMode.Normal)
        {
            switch (key)
            {
                case { Key: ConsoleKey.F5 }:
                    Console.Clear();
                    break;
                case { Key: ConsoleKey.S }:
                    _renderer.HasShadow = !_renderer.HasShadow;

                    break;
                case { Key: ConsoleKey.T }:
                    Console.Clear();
                    _renderer.UseTransparent = !_renderer.UseTransparent;
                    _renderer.Palette[1] = _renderer.UseTransparent
                        ? _transparentShadowColor
                        : _shadowColor;
                    if (_renderer.UseTransparent)
                        _timer.Enabled = _play = false;

                    break;
                case { Key: ConsoleKey.Tab, Modifiers: ConsoleModifiers.Shift }:
                case { Key: ConsoleKey.UpArrow }:
                case { Key: ConsoleKey.LeftArrow }:
                    _index = Math.Max(_index - 1, 0);
                    break;
                case { Key: ConsoleKey.Tab }:
                case { Key: ConsoleKey.DownArrow }:
                case { Key: ConsoleKey.RightArrow }:
                    _index = Math.Min(_index + 1, _renderer.Count - 1);
                    break;
                case { Key: ConsoleKey.Spacebar }:
                    _timer.Enabled = _play = !_timer.Enabled;
                    if (_play)
                        _renderer.UseTransparent = false;

                    break;
                case { Key: ConsoleKey.H }:
                    Console.Clear();
                    _latestHouseColor = _currentHouseColor;
                    var (h, _, _) = _latestHouseColor.ToHSV();
                    _h = (int)h;
                    _mode = ShpViewerMode.HouseColorPicker;
                    break;
                case { Key: ConsoleKey.Home }:
                    _index = 0;
                    break;
                case { Key: ConsoleKey.End }:
                    _index = _renderer.Count - 1;
                    break;
            }
        }
        else if (_mode is ShpViewerMode.HouseColorPicker)
        {
            switch (key)
            {
                case { Key: ConsoleKey.F5 }:
                    Console.Clear();
                    break;
                case { Key: ConsoleKey.S }:
                    _renderer.HasShadow = !_renderer.HasShadow;
                    break;
                case { Key: ConsoleKey.T }:
                    Console.Clear();
                    _renderer.UseTransparent = !_renderer.UseTransparent;
                    _renderer.Palette[1] = _renderer.UseTransparent
                        ? _transparentShadowColor
                        : _shadowColor;
                    if (_renderer.UseTransparent)
                        _timer.Enabled = _play = false;

                    break;
                case { Key: ConsoleKey.UpArrow }:
                case { Key: ConsoleKey.LeftArrow }:
                    _h++;
                    break;
                case { Key: ConsoleKey.DownArrow }:
                case { Key: ConsoleKey.RightArrow }:
                    _h--;
                    break;
                case { Key: ConsoleKey.Escape }:
                case { Key: ConsoleKey.Q }:
                    Console.Clear();
                    _currentHouseColor = _latestHouseColor;
                    _mode = ShpViewerMode.Normal;
                    break;
                case { Key: ConsoleKey.Spacebar }:
                case { Key: ConsoleKey.Enter }:
                    Console.Clear();
                    _mode = ShpViewerMode.Normal;
                    break;
                case { Key: ConsoleKey.Home }:
                    _h = 0;
                    break;
                case { Key: ConsoleKey.End }:
                    _h = 360;
                    break;
            }

            if (_mode is not ShpViewerMode.Normal)
            {
                _h = int.Clamp(_h, 0, 360);
                _currentHouseColor = BGRA32.FromHSV(new(_h, 1, 1));
            }
            _renderer.UpdateHouseColor(_currentHouseColor);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            _timer.Dispose();
        }

        _disposedValue = true;
        base.Dispose(disposing);
    }
}
