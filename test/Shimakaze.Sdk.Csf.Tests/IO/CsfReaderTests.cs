namespace Shimakaze.Sdk.IO.Csf;

[TestClass]
public class CsfReaderTests
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";

    [TestMethod]
    public void ReadTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        Assert.IsNotNull(reader.Read());
    }
}