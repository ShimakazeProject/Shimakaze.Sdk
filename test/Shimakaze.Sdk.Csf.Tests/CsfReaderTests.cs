namespace Shimakaze.Sdk.Csf.Tests;

[TestClass]
public class CsfReaderTests
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";

    [TestMethod]
    public void ReadTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        Assert.IsNotNull(CsfReader.Read(stream));
    }
}