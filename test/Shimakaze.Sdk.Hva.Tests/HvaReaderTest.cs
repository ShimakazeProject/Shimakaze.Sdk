namespace Shimakaze.Sdk.Hva.Tests;

[TestClass]
public sealed class HvaReaderTest
{
    private const string Assets = "Assets";
    private const string InputFile = "jeep.hva";

    [TestMethod]
    public async Task ReadTestAsync()
    {
        using FileStream stream = File.OpenRead(Path.Combine(Assets, InputFile));

        using HvaReader reader = new(stream);

        HvaFile res = await reader.ReadAsync();

        Console.WriteLine(res);
        foreach (HvaFrame a in res.Frames)
        {
            foreach (HvaMatrix item in a.Matrices)
            {
                Console.WriteLine(item);
            }
        }
    }
}