namespace Shimakaze.Sdk.Text.Ini.Serialization.Tests;

[TestClass]
public class IniSerializerTests
{
    private const string Assets = "Assets";
    private const string InputFile = "rulesmd.ini";
    private const string OutputPath = "Out";

    public IniSerializerTests()
    {
        if (!Directory.Exists(OutputPath))
        {
            Directory.CreateDirectory(OutputPath);
        }
    }

    [TestMethod]
    public void DeserializeTest()
    {
        using StreamReader sr = File.OpenText(Path.Combine(Assets, InputFile));
        IniDocument ini = IniSerializer.Deserialize(sr);
        Assert.IsNotNull(ini);
    }

    [TestMethod]
    public void DeserializeTest1()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        IniDocument ini = IniSerializer.Deserialize(stream);
        Assert.IsNotNull(ini);
    }

    [TestMethod]
    public void SerializeTest()
    {
        using StreamReader sr = File.OpenText(Path.Combine(Assets, InputFile));
        IniDocument document = IniSerializer.Deserialize(sr);

        using StreamWriter sw = File.CreateText(Path.Combine(OutputPath, "SerializeTest.ini"));
        IniSerializer.Serialize(sw, document);
    }

    [TestMethod]
    public void SerializeTest1()
    {
        using StreamReader sr = File.OpenText(Path.Combine(Assets, InputFile));
        IniDocument document = IniSerializer.Deserialize(sr);

        using Stream stream = File.Create(Path.Combine(OutputPath, "SerializeTest1.ini"));
        IniSerializer.Serialize(stream, document);
    }
}
