using Shimakaze.Sdk.Ini;

namespace Shimakaze.Sdk.IO.Ini.Serialization;

[TestClass]
public class IniWriterTests
{
    private const string Assets = "Assets";
    private const string InputFile = "normal.ini";
    private const string OutputFile = "SerializeTest.ini";
    private const string OutputPath = "Out";
    private IniDocument? _document;

    [TestMethod]
    public async Task DeserializeAsyncTest()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using IniReader deserializer = new(stream);
        IniDocument ini = await deserializer.ReadAsync();

        Assert.AreNotEqual(0, ini.Count);
        Assert.AreNotEqual(0, ini.First<IniSection>().Count);
    }

    [TestMethod]
    public async Task DeserializeTestAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using IniReader deserializer = new(stream);
        IniDocument ini = await deserializer.ReadAsync();

        Assert.AreNotEqual(0, ini.Count);
        Assert.AreNotEqual(0, ini.First<IniSection>().Count);
    }

    [TestMethod]
    public async Task SerializeAsyncTest()
    {
        Assert.IsNotNull(_document);

        string path = Path.Combine(OutputPath, OutputFile);
        await using var stream = File.Create(path);
        using IniWriter serializer = new(stream);
        await serializer.WriteAsync(_document);
    }

    [TestInitialize]
    public async Task StartupAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using IniReader deserializer = new(stream);
        _document = await deserializer.ReadAsync();

        Directory.CreateDirectory(OutputPath);
    }
}