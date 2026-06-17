using System.Drawing;
using System.Globalization;
using System.Text;

namespace Shimakaze.Sdk.Engine.Cli.Sixel;

// https://github.com/ShaunLawrie/spectre.console/blob/main/src/Extensions/Spectre.Console.ImageSharp/Sixels/Compatibility.cs
internal sealed class ConsoleExt
{
    private static bool? s_terminalSupportsSixel;
    public static bool TerminalSupportsSixel()
    {
        if (s_terminalSupportsSixel.HasValue)
            return s_terminalSupportsSixel.Value;

        s_terminalSupportsSixel = GetControlSequenceResponse("[c").Contains(";4;");
        return s_terminalSupportsSixel.Value;
    }

    private static Size? s_cellSize;
    public static Size GetCellSize()
    {
        if (s_cellSize != null)
            return s_cellSize.Value;

        string response = GetControlSequenceResponse("[16t");

        string[] parts = response.Split(';', 't');
        s_cellSize = new()
        {
            Width = int.Parse(parts[2], CultureInfo.InvariantCulture),
            Height = int.Parse(parts[1], CultureInfo.InvariantCulture),
        };

        return s_cellSize.Value;
    }

    private static string GetControlSequenceResponse(string controlSequence)
    {
        char? c;
        StringBuilder response = new();

        System.Console.Write($"\e{controlSequence}");
        do
        {
            c = System.Console.ReadKey(true).KeyChar;
            response.Append(c);
        }
        while (c != 'c' && System.Console.KeyAvailable);

        return response.ToString();
    }
}
