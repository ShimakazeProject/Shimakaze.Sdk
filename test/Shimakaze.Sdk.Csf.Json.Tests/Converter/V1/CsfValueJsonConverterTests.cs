using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json.Converter.V1;

[TestClass]
public class CsfValueJsonConverterTests
{
    private readonly CsfValueJsonConverter _converter = new();
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
            var reader = new Utf8JsonReader("""null"""u8);
            reader.Read();
            _converter.Read(ref reader, typeof(CsfValue), _options!);
        });
    }

    [TestMethod]
    public void ReadTest2()
    {
        var reader = new Utf8JsonReader("""{"value":"Value"}"""u8);
        reader.Read();
        var value = _converter.Read(ref reader, typeof(CsfValue), _options!);
        Assert.AreEqual("Value", value.Value);
    }

    [TestMethod]
    public void WriteTest()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        // Act
        _converter.Write(writer, new CsfValue("hello", "extra"), _options!);
        writer.Flush();
        stream.Position = 0;

        // Assert
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        Assert.AreEqual("""{"value":"hello","extra":"extra"}""", json);
    }
}