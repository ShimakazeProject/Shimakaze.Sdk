using System.Drawing;
using System.Globalization;
using System.Text;
using System.Timers;

using Shimakaze.Sdk.Engine.Cli.Resources;
using Shimakaze.Sdk.Engine.Cli.TUI;
using Shimakaze.Sdk.Engine.Cli.TUI.Components.Images;
using Shimakaze.Sdk.Engine.Common.Pixels;
using Shimakaze.Sdk.Engine.Shp;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Shp;

using Timer = System.Timers.Timer;

namespace Shimakaze.Sdk.Engine.Cli.App;

internal enum ShpViewerMode
{
    Normal,
    HouseColorPicker
}

internal sealed class ShpViewer(ShapeImage shp, Palette pal) : NanoFramework
{
    private readonly BGRA32 _transparentShadowColor = new(56, 56, 56);
    private readonly Timer _timer = new(TimeSpan.FromSeconds(1 / 30d));
    private readonly ShpRenderer _renderer = new(shp, pal);
    private readonly ImageElement _image = ImageElement.Create();
    private readonly CompositeFormat _hueFormat = CompositeFormat.Parse(Resource.TUI_ShpViewer_HueFormat);
    private bool _play;
    private BGRA32 _pal0;
    private BGRA32 _pal1;
    private BGRA32 _latestHouseColor;
    private BGRA32 _currentHouseColor;
    private bool _disposedValue;
    private int _index;

    private bool UseTransparent
    {
        get;
        set
        {
            field = value;
            if (value)
            {
                _renderer.Palette[0] = BGRA32.Transparent;
                _renderer.Palette[1] = _transparentShadowColor;
            }
            else
            {
                _renderer.Palette[0] = _pal0;
                _renderer.Palette[1] = _pal1;
            }

            UpdateImage(Index);
        }
    }
    private int Hue
    {
        get;
        set
        {
            field = int.Clamp(value, 0, 360);
            if (Mode is not ShpViewerMode.HouseColorPicker)
                return;

            _currentHouseColor = BGRA32.FromHSV(new(Hue, 1, 1));
            _renderer.UpdateHouseColor(_currentHouseColor);
            UpdateImage(Index);
        }
    }
    private int Index
    {
        get => _index;
        set => UpdateImage(value);
    }

    private ShpViewerMode Mode
    {
        get => (ShpViewerMode)ShortKeyManager.CurrentLayer;
        set => ShortKeyManager.CurrentLayer = (int)value;
    }

    protected override void OnInitialize()
    {
        _pal0 = _renderer.Palette[0];
        _pal1 = _renderer.Palette[1];
        _timer.Elapsed += TimerElapsed;
        base.OnInitialize();
        Index = 0;
        TitleBar.Left = "Shimakaze.Sdk.ShpViewer";

        ShortKeyManager.Regist((int)ShpViewerMode.Normal, new ShortKey(ConsoleKey.F5, ConsoleModifiers.None), Console.Clear);
        ShortKeyManager.Regist((int)ShpViewerMode.Normal, new ShortKey(ConsoleKey.Escape, ConsoleModifiers.None), Exit);
        ShortKeyManager.Regist((int)ShpViewerMode.Normal, new NamedShortKey(ConsoleKey.Q, ConsoleModifiers.None, Resource.TUI_ShpViewer_Exit), Exit);
        ShortKeyManager.Regist((int)ShpViewerMode.Normal, new NamedSwitchShortKey(ConsoleKey.Spacebar, ConsoleModifiers.None, i => i ? Resource.TUI_ShpViewer_Pause : Resource.TUI_ShpViewer_Play, () => _play), SwitchPlayPause);
        ShortKeyManager.Regist((int)ShpViewerMode.Normal, new NamedShortKey(ConsoleKey.H, ConsoleModifiers.None, Resource.TUI_ShpViewer_ToggleHouse), EnterHouseMode);
        ShortKeyManager.Regist((int)ShpViewerMode.Normal, new ShortKey(ConsoleKey.Tab, ConsoleModifiers.Shift), PrevFrame);
        ShortKeyManager.Regist((int)ShpViewerMode.Normal, new ShortKey(ConsoleKey.UpArrow, ConsoleModifiers.None), PrevFrame);
        ShortKeyManager.Regist((int)ShpViewerMode.Normal, new NamedShortKey(ConsoleKey.LeftArrow, ConsoleModifiers.None, Resource.TUI_ShpViewer_PrevFrame), PrevFrame);
        ShortKeyManager.Regist((int)ShpViewerMode.Normal, new ShortKey(ConsoleKey.Tab, ConsoleModifiers.None), NextFrame);
        ShortKeyManager.Regist((int)ShpViewerMode.Normal, new ShortKey(ConsoleKey.DownArrow, ConsoleModifiers.None), NextFrame);
        ShortKeyManager.Regist((int)ShpViewerMode.Normal, new NamedShortKey(ConsoleKey.RightArrow, ConsoleModifiers.None, Resource.TUI_ShpViewer_NextFrame), NextFrame);
        ShortKeyManager.Regist((int)ShpViewerMode.Normal, new NamedSwitchShortKey(ConsoleKey.S, ConsoleModifiers.None, i => i ? Resource.TUI_ShpViewer_DisableShadow : Resource.TUI_ShpViewer_EnableShadow, () => _renderer.HasShadow), SwitchShadow);
        ShortKeyManager.Regist((int)ShpViewerMode.Normal, new NamedSwitchShortKey(ConsoleKey.T, ConsoleModifiers.None, i => i ? Resource.TUI_ShpViewer_DisableTransparent : Resource.TUI_ShpViewer_EnableTransparent, () => UseTransparent), SwitchTransparent);
        ShortKeyManager.Regist((int)ShpViewerMode.Normal, new ShortKey(ConsoleKey.Home, ConsoleModifiers.None), () => Index = 0);
        ShortKeyManager.Regist((int)ShpViewerMode.Normal, new ShortKey(ConsoleKey.End, ConsoleModifiers.None), () => Index = _renderer.Count - 1);

        ShortKeyManager.Regist((int)ShpViewerMode.HouseColorPicker, new ShortKey(ConsoleKey.F5, ConsoleModifiers.None), Console.Clear);
        ShortKeyManager.Regist((int)ShpViewerMode.HouseColorPicker, new NamedSwitchShortKey(ConsoleKey.S, ConsoleModifiers.None, i => i ? Resource.TUI_ShpViewer_DisableShadow : Resource.TUI_ShpViewer_EnableShadow, () => _renderer.HasShadow), SwitchShadow);
        ShortKeyManager.Regist((int)ShpViewerMode.HouseColorPicker, new NamedSwitchShortKey(ConsoleKey.T, ConsoleModifiers.None, i => i ? Resource.TUI_ShpViewer_DisableTransparent : Resource.TUI_ShpViewer_EnableTransparent, () => UseTransparent), SwitchTransparent);
        ShortKeyManager.Regist((int)ShpViewerMode.HouseColorPicker, new NamedShortKey(ConsoleKey.UpArrow, ConsoleModifiers.None, Resource.TUI_ShpViewer_HueIncrease), HueIncrease);
        ShortKeyManager.Regist((int)ShpViewerMode.HouseColorPicker, new ShortKey(ConsoleKey.LeftArrow, ConsoleModifiers.None), HueIncrease);
        ShortKeyManager.Regist((int)ShpViewerMode.HouseColorPicker, new NamedShortKey(ConsoleKey.DownArrow, ConsoleModifiers.None, Resource.TUI_ShpViewer_HueDecrease), HueDecrease);
        ShortKeyManager.Regist((int)ShpViewerMode.HouseColorPicker, new ShortKey(ConsoleKey.RightArrow, ConsoleModifiers.None), HueDecrease);
        ShortKeyManager.Regist((int)ShpViewerMode.HouseColorPicker, new ShortKey(ConsoleKey.Escape, ConsoleModifiers.None), ExitHouseMode);
        ShortKeyManager.Regist((int)ShpViewerMode.HouseColorPicker, new NamedShortKey(ConsoleKey.Q, ConsoleModifiers.None, Resource.TUI_ShpViewer_ExitHouseMode), ExitHouseMode);
        ShortKeyManager.Regist((int)ShpViewerMode.HouseColorPicker, new ShortKey(ConsoleKey.Spacebar, ConsoleModifiers.None), ChangeHouseColor);
        ShortKeyManager.Regist((int)ShpViewerMode.HouseColorPicker, new NamedShortKey(ConsoleKey.Enter, ConsoleModifiers.None, Resource.TUI_ShpViewer_SelectColor), ChangeHouseColor);
        ShortKeyManager.Regist((int)ShpViewerMode.HouseColorPicker, new ShortKey(ConsoleKey.Home, ConsoleModifiers.None), () => Hue = 0);
        ShortKeyManager.Regist((int)ShpViewerMode.HouseColorPicker, new ShortKey(ConsoleKey.End, ConsoleModifiers.None), () => Hue = 360);

    }

    private void Exit()
    {
        Shutdown();
    }

    private async void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (Index + 1 >= _renderer.Count)
            Index = 0;
        else
            Index++;
        await SendEvent(e);
    }

    private void HueIncrease()
    {
        Hue++;
    }

    private void HueDecrease()
    {
        Hue--;
    }

    private void ExitHouseMode()
    {
        Console.Clear();
        _currentHouseColor = _latestHouseColor;
        Mode = ShpViewerMode.Normal;
    }

    private void ChangeHouseColor()
    {
        Console.Clear();
        Mode = ShpViewerMode.Normal;
    }

    private void SwitchPlayPause()
    {
        _timer.Enabled = _play = !_timer.Enabled;
        if (_play)
            UseTransparent = false;
    }

    private void EnterHouseMode()
    {
        Console.Clear();
        _latestHouseColor = _currentHouseColor;
        Mode = ShpViewerMode.HouseColorPicker;
        var (h, _, _) = _latestHouseColor.ToHSV();
        Hue = (int)h;
    }

    private void PrevFrame()
    {
        Index = Math.Max(Index - 1, 0);
    }

    private void NextFrame()
    {
        Index = Math.Min(Index + 1, _renderer.Count - 1);
    }

    private void SwitchShadow()
    {
        _renderer.HasShadow = !_renderer.HasShadow;
        UpdateImage(Index);
    }

    private void UpdateImage(int index)
    {
        _index = int.Clamp(index, 0, _renderer.Count - 1);
        _image.Image = _renderer.GetFrame(_index).RenderAsImage();
    }

    private void SwitchTransparent()
    {
        UseTransparent = !UseTransparent;
        if (UseTransparent)
            _timer.Enabled = _play = false;
    }

    protected override void OnRender()
    {
        if (UseTransparent)
            Console.Clear();

        HelpBar.Title = Mode switch
        {
            ShpViewerMode.Normal => $"[ {(_play ? "⏸" : "▶")}  \e[32m{Index + 1}\e[0m / \e[32m{_renderer.Count}\e[0m ]",
            ShpViewerMode.HouseColorPicker => $"{string.Format(CultureInfo.InvariantCulture, _hueFormat, Hue)} [\e[38;2;{_currentHouseColor.R};{_currentHouseColor.G};{_currentHouseColor.B}m     \e[0m]",
            _ => string.Empty,
        };

        TitleBar.Right = Mode switch
        {
            ShpViewerMode.HouseColorPicker => $"House",
            _ => string.Empty,
        };

        base.OnRender();

        if (_image is GDI32Image)
        {
            Size size = new(Console.WindowWidth, Console.WindowHeight);
            var imageSize = _image.Measure(size);
            var p = (size.Width - imageSize.Width) / 2;
            Console.SetCursorPosition(p, 1);
            _image.OnRender(Console.Out, imageSize);
        }
    }

    public override void OnClientRender(TextWriter writer, Size size)
    {
        var imageSize = _image.Measure(size);
        var p = (size.Width - imageSize.Width) / 2;
        Console.SetCursorPosition(p, 1);
        writer.Write($"\e[{p + 1}G");
        _image.OnRender(writer, imageSize);
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            _timer.Dispose();
            _image.Dispose();
        }

        _disposedValue = true;
        base.Dispose(disposing);
    }
}
