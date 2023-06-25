using Shimakaze.Sdk.Ini;

namespace Shimakaze.Sdk.IO.Ini.Serialization;

[TestClass]
public class IniSerializerTests
{
    private const string Assets = "Assets";
    private const string InputFile = "normal.ini";
    private const string OutputPath = "Out";
    private const string OutputFile1 = "SerializeTest1.ini";
    private const string OutputFile2 = "SerializeTest2.ini";
    private IniDocument? _document;

    [TestInitialize]
    public async Task StartupAsync()
    {
        using var stream = File.OpenText(Path.Combine(Assets, InputFile));
        using IniDeserializer deserializer = new(stream);
        _document = await deserializer.DeserializeAsync();

        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public void DeserializeTest()
    {
        using var stream = File.OpenText(path: Path.Combine(Assets, InputFile));
        using IniDeserializer deserializer = new(stream);
        IniDocument ini = deserializer.Deserialize();

        Assert.AreNotEqual(0, ini.Count);
        Assert.AreNotEqual(0, ini.First<IniSection>().Count);
    }

    [TestMethod]
    public async Task DeserializeAsyncTest()
    {
        using var stream = File.OpenText(path: Path.Combine(Assets, InputFile));
        using IniDeserializer deserializer = new(stream);
        IniDocument ini = await deserializer.DeserializeAsync();

        Assert.AreNotEqual(0, ini.Count);
        Assert.AreNotEqual(0, ini.First<IniSection>().Count);
    }

    [TestMethod]
    public void SerializeTest()
    {
        Assert.IsNotNull(_document);

        string path = Path.Combine(OutputPath, OutputFile1);
        using var stream = File.CreateText(path);
        using IniSerializer serializer = new(stream);
        serializer.Serialize(_document);
    }

    [TestMethod]
    public async Task SerializeAsyncTest()
    {
        Assert.IsNotNull(_document);

        string path = Path.Combine(OutputPath, OutputFile2);
        await using var stream = File.CreateText(path);
        using IniSerializer serializer = new(stream);
        await serializer.SerializeAsync(_document);
    }
}