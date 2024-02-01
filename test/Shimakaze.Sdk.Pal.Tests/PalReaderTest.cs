using Shimakaze.Sdk;

namespace Shimakaze.Sdk.Pal.Tests;

[TestClass]
public sealed class PalReaderTest
{
    private const string Assets = "Assets";
    private const string InputFile = "unittem.pal";

    [TestMethod]
    public void ReadTest()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));

        var res = PaletteReader.Read(stream);
    }
}