

namespace Shimakaze.Sdk.IO.Hva.Tests;


[TestClass]
public sealed class HvaReaderTest
{
    private const string Assets = "Assets";
    private const string InputFile = "jeep.hva";


    [TestMethod]
    public void ReadTest()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));

        using HvaReader reader = new(stream);

        var res = reader.Read();

        Console.WriteLine(res);
        foreach (var a in res.Frames)
            foreach (var item in a.Matrices)
                Console.WriteLine(item);
    }
}