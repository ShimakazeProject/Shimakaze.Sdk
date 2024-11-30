using System.Text.Json;

using Shimakaze.Sdk.Csf.Json.Converter.V2;

namespace Shimakaze.Sdk.Csf.Json.Tests.Converter.V2;

[TestClass]
public class CsfDataValueJsonConverterTests
{
    private readonly CsfDataValueJsonConverter _converter = new();
    private JsonSerializerOptions? _options;

    [TestMethod]
    public void ReadTest()
    {
        Utf8JsonReader reader = new("""
        {
            "values": [
                "hello"
            ],
            "test": null
        }
        """u8);
        reader.Read();
        IList<CsfValue>? value = _converter.Read(ref reader, typeof(IList<CsfValue>), _options!);
        Assert.IsNotNull(value);
        Assert.AreEqual(1, value.Count);
        Assert.AreEqual("hello", value[0].Value);
    }

    [TestMethod]
    public void ReadTest2()
    {
        Utf8JsonReader reader = new("""
        {
            "value": "hello",
            "test": null
        }
        """u8);
        reader.Read();
        IList<CsfValue>? value = _converter.Read(ref reader, typeof(IList<CsfValue>), _options!);
        Assert.IsNotNull(value);
        Assert.AreEqual(1, value.Count);
        Assert.AreEqual("hello", value[0].Value);
    }

    [TestInitialize]
    public void Startup()
    {
        _options ??= new();
        foreach (System.Text.Json.Serialization.JsonConverter item in CsfJsonSerializerOptions.Converters)
        {
            _options.Converters.Add(item);
        }
    }

    [TestMethod]
    public void WriteTest()
    {
        // Arrange
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream);
        // Act
        _converter.Write(writer, [
            new("hello"),
            new("world"),
        ], _options!);
        writer.Flush();
        stream.Position = 0;

        // Assert
        using StreamReader reader = new(stream);
        string json = reader.ReadToEnd();
        Assert.AreEqual("""{"values":["hello","world"]}""", json);
    }
}
