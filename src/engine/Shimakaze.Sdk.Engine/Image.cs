using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Engine;

internal readonly record struct BGRA32(byte B, byte G, byte R, byte A)
{
    internal static readonly BGRA32 Transparent = new(0, 0, 0, 0);

    public BGRA32(byte b, byte g, byte r) : this(b, g, r, byte.MaxValue)
    {
    }

    public BGRA32(uint value) : this((byte)((value & 0xFF000000) >> 24), (byte)((value & 0x00FF0000) >> 16), (byte)((value & 0x0000FF00) >> 8), (byte)(value & 0x000000FF))
    {
    }

    public static implicit operator BGRA32(DisplayColor color) => new(color.Blue, color.Green, color.Red);

    public bool Equals(BGRA32 bGRA)
    {
        return B == bGRA.B &&
               G == bGRA.G &&
               R == bGRA.R &&
               A == bGRA.A;
    }

    public override int GetHashCode()
    {
        int hashCode = 931614316;
        hashCode = hashCode * -1521134295 + B.GetHashCode();
        hashCode = hashCode * -1521134295 + G.GetHashCode();
        hashCode = hashCode * -1521134295 + R.GetHashCode();
        hashCode = hashCode * -1521134295 + A.GetHashCode();
        return hashCode;
    }
}

internal sealed record class Image(int Width, int Height, ImmutableArray<BGRA32> Pixels)
{
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

        var output = pInfo.StandardOutput.ReadToEnd().Split(':', 2);
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
