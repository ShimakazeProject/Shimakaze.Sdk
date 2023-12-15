using System.Globalization;
using System.Security.Cryptography;

using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Pal.Tests;

[TestClass]
public sealed class PalWriterTest
{
    private const string Assets = "Assets";
    private const string InputFile = "unittem.pal";
    private const string OutputFile = "unittem.pal";
    private const string OutputPath = "Out";
    private Palette _pal = default!;

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));

        using PaletteReader reader = new(stream);

        _pal = reader.Read();
    }

    [TestMethod]
    public void WriteTest()
    {
        using (Stream stream = File.Create(Path.Combine(OutputPath, OutputFile)))
        using (PaletteWriter writer = new(stream))
            writer.Write(_pal);

        Assert.IsTrue(Compare(Path.Combine(Assets, InputFile), Path.Combine(OutputPath, OutputFile)));
    }

    private bool Compare(string path1, string path2)
    {
        Span<byte> buffer1 = stackalloc byte[8];
        Span<byte> buffer2 = stackalloc byte[8];

        using Stream fs1 = File.OpenRead(Path.Combine(Assets, InputFile));
        using Stream fs2 = File.OpenRead(Path.Combine(OutputPath, OutputFile));
        if (fs1.Length != fs2.Length)
            return false;

        while (fs1.Position < fs1.Length)
        {
            fs1.Read(buffer1);
            fs2.Read(buffer2);
            if (!buffer1.SequenceEqual(buffer2))
                return false;
        }
        return true;
    }
}