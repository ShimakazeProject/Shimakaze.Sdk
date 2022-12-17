namespace Shimakaze.Sdk.Text.Ini.Tests;

[TestClass]
public class IniReaderTests
{
    private const string Assets = "Assets";
    private const string InputFile = "rulesmd.ini";

    [TestMethod]
    public void IniReaderTest()
    {
        using StreamReader sr = File.OpenText(Path.Combine(Assets, InputFile));
        IniReader reader = new(sr);
    }

    [TestMethod]
    public void ReaderTest()
    {
        using StreamReader sr = File.OpenText(Path.Combine(Assets, InputFile));
        IniReader reader = new(sr);
        while (reader.Read())
        {
            Assert.IsNotNull(reader.Token);
            Console.WriteLine(reader.Value);
        }
    }
}
