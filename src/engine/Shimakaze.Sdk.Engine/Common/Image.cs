using System.Collections.Immutable;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;

using Shimakaze.Sdk;

namespace Shimakaze.Sdk.Engine.Common;

internal sealed record class Image(int Width, int Height, ImmutableArray<BGRA32> Pixels)
{
    internal Image(Size size, BGRA32[] pixels) : this(size.Width, size.Height, ImmutableCollectionsMarshal.AsImmutableArray(pixels))
    { }

    public BGRA32 GetPixel(int x, int y) => Pixels[(y * Width) + x];

    public static Image Load(string path)
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
        stream.Write(Pixels.AsSpan());
        stream.Flush();
#pragma warning disable IDISP007 // Don't dispose injected
        stream.Dispose();
#pragma warning restore IDISP007 // Don't dispose injected

        pData.WaitForExit();

        Debug.Assert(pData.ExitCode is 0);
    }
}
