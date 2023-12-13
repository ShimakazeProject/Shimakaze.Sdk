namespace Shimakaze.Sdk.Pal.Tests;

[TestClass]
public sealed class PalReaderTest
{
    private const string Assets = "Assets";
    private const string InputFile = "unittem.pal";

    [TestMethod]
    public async Task ReadTestAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));

        using PaletteReader reader = new(stream);

        var res = await reader.ReadAsync();
    }
}