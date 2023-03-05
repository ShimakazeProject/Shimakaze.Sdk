using Shimakaze.Sdk.Text.Ini;
using Shimakaze.Sdk.Text.Ini.Serialization;

namespace Shimakaze.Sdk.Tests.Text.Ini.Serialization;

[TestClass]
public class IniSerializerTests
{
    private const string Assets = "Assets";
    private const string InputFile = "rulesmd.ini";
    private const string OutputPath = "Out";
    private const string OutputFile1 = "SerializeTest1.ini";
    private const string OutputFile2 = "SerializeTest2.ini";

    public IniSerializerTests()
    {
        Directory.CreateDirectory(OutputPath);
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
    public void SerializeTest1()
    {
        using StreamReader sr = File.OpenText(Path.Combine(Assets, InputFile));
        IniDocument document = IniSerializer.Deserialize(sr);

        using StreamWriter sw = File.CreateText(Path.Combine(OutputPath, OutputFile1));
        IniSerializer.Serialize(sw, document);
    }

    [TestMethod]
    public void SerializeTest2()
    {
        using StreamReader sr = File.OpenText(Path.Combine(Assets, InputFile));
        IniDocument document = IniSerializer.Deserialize(sr);

        using Stream stream = File.Create(Path.Combine(OutputPath, OutputFile2));
        IniSerializer.Serialize(stream, document);
    }
}
