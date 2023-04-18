using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json.Converter.V2;

[TestClass]
public class CsfDataValueJsonConverterTests
{
    private readonly CsfDataValueJsonConverter _converter = new();
    private JsonSerializerOptions? _options;

    [TestInitialize]
    public void Startup()
    {
        _options ??= new();
        foreach (var item in CsfJsonSerializerOptions.Converters)
            _options.Converters.Add(item);
    }

    [TestMethod]
    public void ReadTest()
    {
        Assert.ThrowsException<JsonException>(() =>
        {
            var reader = new Utf8JsonReader("""true"""u8);
            reader.Read();
            _converter.Read(ref reader, typeof(IList<CsfValue>), _options!);
        });
    }

    [TestMethod]
    public void ReadTest1()
    {
        var reader = new Utf8JsonReader("""
        {
            "values": [
                "hello"
            ],
            "test": null
        }
        """u8);
        reader.Read();
        var value = _converter.Read(ref reader, typeof(IList<CsfValue>), _options!);
        Assert.IsNotNull(value);
        Assert.AreEqual(1, value.Count);
        Assert.AreEqual("hello", value[0].Value);
    }

    [TestMethod]
    public void ReadTest2()
    {
        var reader = new Utf8JsonReader("""
        {
            "value": "hello",
            "test": null
        }
        """u8);
        reader.Read();
        var value = _converter.Read(ref reader, typeof(IList<CsfValue>), _options!);
        Assert.IsNotNull(value);
        Assert.AreEqual(1, value.Count);
        Assert.AreEqual("hello", value[0].Value);
    }

    [TestMethod]
    public void WriteTest()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        // Act
        _converter.Write(writer, new List<CsfValue>() {
            new("hello"),
            new("world"),
        }, _options!);
        writer.Flush();
        stream.Position = 0;

        // Assert
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        Assert.AreEqual("""{"values":["hello","world"]}""", json);
    }
}