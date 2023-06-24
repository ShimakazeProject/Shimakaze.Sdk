using System.Security.Cryptography;

using Shimakaze.Sdk.Pal;
namespace Shimakaze.Sdk.IO.Pal.Tests;


[TestClass]
public sealed class PalWriterTest
{
    private const string OutputPath = "Out";
    private const string OutputFile = "unittem.pal";

    private const string Assets = "Assets";
    private const string InputFile = "unittem.pal";

    private Palette _pal;

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

        var a = BitConverter.ToString(MD5.HashData(File.ReadAllBytes(Path.Combine(Assets, InputFile))));
        var b = BitConverter.ToString(MD5.HashData(File.ReadAllBytes(Path.Combine(OutputPath, OutputFile))));

        Assert.AreEqual(a, b, true);
    }
}