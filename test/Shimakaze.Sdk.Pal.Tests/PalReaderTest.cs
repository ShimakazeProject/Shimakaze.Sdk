namespace Shimakaze.Sdk.Pal.Tests;

[TestClass]
public sealed class PalReaderTest
{
    private const string Assets = "Assets";
    private const string InputFile = "unittem.pal";

    [TestMethod]
    public void ReadTest()
    {
        using FileStream stream = File.OpenRead(Path.Combine(Assets, InputFile));

        Palette res = PaletteReader.Read(stream);
    }
}
