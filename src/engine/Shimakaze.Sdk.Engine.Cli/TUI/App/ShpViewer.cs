using System.Drawing;
using System.Globalization;
using System.Text;
using System.Timers;

using Shimakaze.Sdk.Engine.Cli.Components;
using Shimakaze.Sdk.Engine.Cli.Resources;
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
    private readonly CompositeFormat _hueFormat = CompositeFormat.Parse(Resource.TUI_ShpViewer_HueFormat);
    private readonly StringBuilder _buffer;
    private readonly StringWriter _writer;
    private readonly ShpImage _shpImage;
    private readonly Timer _timer;
    private bool _play;
    private ShpViewerMode _mode;
    private Color _latestHouseColor;
    private int _h;
    private bool _disposedValue;

    public ShpViewer(ShapeImage shp, Palette pal)
    {
        _buffer = new();
        _writer = new(_buffer);

        _timer = new(TimeSpan.FromSeconds(1 / 30d));
        _timer.Elapsed += TimerElapsed;
        _shpImage = new(shp, pal)
        {
            UseTransparent = false,
            Center = true,
        };
    }

    private async void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        await base.SendEvent(e, default);
    }

    protected override void OnEvent(EventArgs eventArgs)
    {
        switch (eventArgs)
        {
            case ConsoleKeyEventArgs keyEventArgs:
                OnKeyEvent(keyEventArgs.KeyInfo);
                break;
            case ElapsedEventArgs:
                if (_shpImage.Index + 1 >= _shpImage.Max)
                    _shpImage.Index = 0;
                else
                    _shpImage.Index++;
                break;
            default:
                break;
        }

    }

    private void WriteHelp(TextWriter writer)
    {
        if (_mode is ShpViewerMode.Normal)
        {
            string l1 = $"{(_play ? "▶" : "⏸")}  \e[32m{_shpImage.Index + 1}\e[0m / \e[32m{_shpImage.Max + 1}\e[0m";
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
                _shpImage.HasShadow ? $"\e[92m\e[47m S \e[0m {Resource.TUI_ShpViewer_DisableShadow}" : $"\e[30m\e[47m S \e[0m {Resource.TUI_ShpViewer_EnableShadow}",
                _shpImage.UseTransparent ? $"\e[92m\e[47m T \e[0m {Resource.TUI_ShpViewer_DisableTransparent}" : $"\e[30m\e[47m T \e[0m {Resource.TUI_ShpViewer_EnableTransparent}",
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
            var c = _shpImage.HouseColor;
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
                _shpImage.HasShadow ? $"\e[92m\e[47m S \e[0m {Resource.TUI_ShpViewer_DisableShadow}" : $"\e[30m\e[47m S \e[0m {Resource.TUI_ShpViewer_EnableShadow}",
                _shpImage.UseTransparent ? $"\e[92m\e[47m T \e[0m {Resource.TUI_ShpViewer_DisableTransparent}" : $"\e[30m\e[47m T \e[0m {Resource.TUI_ShpViewer_EnableTransparent}",
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
        int i = Console.WindowHeight - 1;
        _buffer.Clear();
        if (_shpImage.UseTransparent)
            _writer.Write("\e[2J");
        _writer.Write("\e[1;1H");
        _writer.Write(_shpImage);
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
                    _shpImage.HasShadow = !_shpImage.HasShadow;


                    break;
                case { Key: ConsoleKey.T }:
                    Console.Clear();
                    _shpImage.UseTransparent = !_shpImage.UseTransparent;
                    _shpImage.ShadowColor = _shpImage.UseTransparent
                        ? Color.FromArgb(56, 56, 56)
                        : null;
                    if (_shpImage.UseTransparent)
                        _timer.Enabled = _play = false;

                    break;
                case { Key: ConsoleKey.Tab, Modifiers: ConsoleModifiers.Shift }:
                case { Key: ConsoleKey.UpArrow }:
                case { Key: ConsoleKey.LeftArrow }:
                    _shpImage.Index--;
                    break;
                case { Key: ConsoleKey.Tab }:
                case { Key: ConsoleKey.DownArrow }:
                case { Key: ConsoleKey.RightArrow }:
                    _shpImage.Index++;
                    break;
                case { Key: ConsoleKey.Spacebar }:
                    _timer.Enabled = _play = !_timer.Enabled;
                    if (_play)
                        _shpImage.UseTransparent = false;

                    break;
                case { Key: ConsoleKey.H }:
                    Console.Clear();
                    _latestHouseColor = _shpImage.HouseColor;
                    _h = (int)_latestHouseColor.GetHue();
                    _mode = ShpViewerMode.HouseColorPicker;
                    break;
                default:
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
                    _shpImage.HasShadow = !_shpImage.HasShadow;
                    break;
                case { Key: ConsoleKey.T }:
                    Console.Clear();
                    _shpImage.UseTransparent = !_shpImage.UseTransparent;
                    if (_shpImage.UseTransparent)
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
                    _shpImage.HouseColor = _latestHouseColor;
                    _mode = ShpViewerMode.Normal;
                    break;
                case { Key: ConsoleKey.Spacebar }:
                case { Key: ConsoleKey.Enter }:
                    Console.Clear();
                    _mode = ShpViewerMode.Normal;
                    break;
                default:
                    break;
            }

            _h = int.Clamp(_h, 0, 359);
            _shpImage.HouseColor = Color.FromHsv(_h, 1, 1);
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
