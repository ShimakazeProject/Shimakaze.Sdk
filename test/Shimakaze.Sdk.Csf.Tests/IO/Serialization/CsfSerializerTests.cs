using Shimakaze.Sdk.Csf;

namespace Shimakaze.Sdk.IO.Csf.Serialization;

[TestClass]
public class CsfSerializerTests
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";
    private const string OutputPath = "Out";
    private const string OutputFile1 = "SerializeTest1.csf";
    private const string OutputFile2 = "SerializeTest2.csf";
    private CsfDocument? _document;

    [TestInitialize]
    public async Task StartupAsync()
    {
        await using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        await using CsfDeserializer deserializer = new(stream);
        _document = await deserializer.DeserializeAsync();

        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public void DeserializeTest()
    {
        using Stream stream = File.OpenRead(path: Path.Combine(Assets, InputFile));
        using CsfDeserializer deserializer = new(stream);
        CsfDocument csf = deserializer.Deserialize();
    }

    [TestMethod]
    public async Task DeserializeAsyncTest()
    {
        await using Stream stream = File.OpenRead(path: Path.Combine(Assets, InputFile));
        await using CsfDeserializer deserializer = new(stream);
        CsfDocument csf = await deserializer.DeserializeAsync();
        await deserializer.DisposeAsync();
    }

    [TestMethod]
    public void SerializeTest()
    {
        Assert.IsNotNull(_document);

        string path = Path.Combine(OutputPath, OutputFile1);
        using Stream stream = File.Create(path);
        using CsfSerializer serializer = new(stream);
        serializer.Serialize(_document);
    }

    [TestMethod]
    public async Task SerializeAsyncTest()
    {
        Assert.IsNotNull(_document);

        string path = Path.Combine(OutputPath, OutputFile2);
        await using Stream stream = File.Create(path);
        await using CsfSerializer serializer = new(stream);
        await serializer.SerializeAsync(_document);
        await serializer.DisposeAsync();
    }
}
