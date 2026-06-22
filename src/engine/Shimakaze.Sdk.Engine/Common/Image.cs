using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

using Shimakaze.Sdk;
using Shimakaze.Sdk.Engine.Common.Pixels;

namespace Shimakaze.Sdk.Engine.Common;

internal abstract record class Image(int Width, int Height)
{
    public abstract BGRA32 GetPixel(int x, int y);

    public static SoftwareImage Load(string path)
    {
        ProcessStartInfo info = new()
        {
            FileName = "magick",
            Arguments = $"identify -format \"%w:%h\" \"{path}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        ProcessStartInfo data = new()
        {
            FileName = "magick",
            Arguments = $"\"{path}\" -depth 8 BGRA:-",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var pInfo = Process.Start(info) ?? throw new InvalidOperationException("Cannot start magick");
        Debug.WriteLine(pInfo.StartInfo.Arguments);

        using var pData = Process.Start(data) ?? throw new InvalidOperationException("Cannot start magick");
        Debug.WriteLine(pData.StartInfo.Arguments);

        using MemoryStream ms = new();

        string[] output = pInfo.StandardOutput.ReadToEnd().Split(':', 2);
        pData.StandardOutput.BaseStream.CopyTo(ms);

        pInfo.WaitForExit();
        Debug.Assert(pInfo.ExitCode is 0);

        pData.WaitForExit();
        Debug.Assert(pData.ExitCode is 0);

        int w = int.Parse(output[0], CultureInfo.InvariantCulture);
        int h = int.Parse(output[1], CultureInfo.InvariantCulture);

        var pixels = MemoryMarshal.Cast<byte, BGRA32>(ms.GetBuffer()).ToArray();

        return new(w, h, ImmutableCollectionsMarshal.AsImmutableArray(pixels));
    }

    public void SaveTo(string path, string arguments = "")
    {
        var image = ToSoftware();

        ProcessStartInfo data = new()
        {
            FileName = "magick",
            Arguments = $"-size {Width}x{Height} -depth 8 BGRA:- {arguments} \"{path}\"",
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var pData = Process.Start(data) ?? throw new InvalidOperationException("Cannot start magick");
        Debug.WriteLine(pData.StartInfo.Arguments);

        var stream = pData.StandardInput.BaseStream;
        stream.Write(image.Pixels.AsSpan());
        stream.Flush();
        stream.Close();

        pData.WaitForExit();

        Debug.Assert(pData.ExitCode is 0);
    }

    public abstract SoftwareImage ToSoftware();
    public abstract PaletteImage ToPalette(int count);
}
