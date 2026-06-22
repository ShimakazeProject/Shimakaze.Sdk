using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.Versioning;
using System.Text;

using Windows.Win32;
using Windows.Win32.System.Console;

namespace Shimakaze.Sdk.Engine.Cli.TUI;

static partial class TerminalImageSupport
{
    public static bool Kitty { get; private set; }
    public static bool ITerm { get; private set; }
    public static bool Sixel { get; private set; }
    [SupportedOSPlatformGuard("windows5.0")]
    public static bool GDI32 { get; private set; }

    public static Size CellSize { get; private set; }

    private static string Query(string command)
    {
        Console.Write(command);
        if (SpinWait.SpinUntil(() => Console.KeyAvailable, 500))
        {
            StringBuilder sb = new();

            while (Console.KeyAvailable)
                sb.Append(Console.ReadKey(true).KeyChar);

            return sb.ToString();
        }

        return string.Empty;
    }

    public static void Detect()
    {
        // Test GDI
        if (OperatingSystem.IsWindowsVersionAtLeast(5))
        {
            if (PInvoke.GetConsoleWindow() is { IsNull: false } hwnd)
            {
                Span<char> className = stackalloc char[256];
                var len = PInvoke.GetClassName(hwnd, className);
                GDI32 = className[..len].Equals("ConsoleWindowClass", StringComparison.OrdinalIgnoreCase);
            }
        }

        var da = Query("\e[c");
        // Test Sixel
        if (!string.IsNullOrWhiteSpace(da))
        {
            var daarr = da[3..^1].Split(';');
            Sixel = ((ICollection<string>)daarr).Contains("4");
        }

        // Test Kitty
        var kitty = Query("\e_Gi=1,a=q,t=d,f=100;AAAA\e\\");
        Kitty = !string.IsNullOrWhiteSpace(kitty) && kitty.StartsWith("\e_Gi", StringComparison.Ordinal);

        if (Sixel || Kitty)
        {
            var res = Query("\e[16t");
            var arr = res[3..^1].Split(';');
            var h = int.Parse(arr[1], CultureInfo.InvariantCulture);
            var w = int.Parse(arr[2], CultureInfo.InvariantCulture);
            CellSize = new(w, h);
        }
        else if (GDI32)
        {
            using var hConsole = PInvoke.GetStdHandle_SafeHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
            if (hConsole.IsInvalid)
                throw new Win32Exception();

            if (!PInvoke.GetCurrentConsoleFont(hConsole, false, out var consoleFontInfo))
                throw new Win32Exception();

            var size = PInvoke.GetConsoleFontSize(hConsole, consoleFontInfo.nFont);
            if (size is { X: 0, Y: 0 })
                throw new Win32Exception();

            CellSize = new(size.X, size.Y);
        }

        // ----- 输出结果 -----
        // var result = $"""
        // Kitty: {Text(Kitty)}
        // iTerm: Unknown
        // Sixel: {Text(Sixel)}
        // GDI32: {Text(GDI32)}
        // """;
        Console.Clear();
    }

    private static string Text(bool b) => b ? "Supported" : "Unsupported";
}

