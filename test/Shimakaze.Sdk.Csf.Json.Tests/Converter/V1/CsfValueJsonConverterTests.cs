using System.Text.Json;

using Shimakaze.Sdk.Csf.Json.Converter.V1;

namespace Shimakaze.Sdk.Csf.Json.Tests.Converter.V1;

[TestClass]
public class CsfValueJsonConverterTests
{
    private readonly CsfValueJsonConverter _converter = new();
    private JsonSerializerOptions? _options;

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
    public void ReadTest()
    {
        Utf8JsonReader reader = new("""{"value":"Value"}"""u8);
        reader.Read();
        CsfValue value = _converter.Read(ref reader, typeof(CsfValue), _options!);
        Assert.AreEqual("Value", value.Value);
    }

    [TestMethod]
    public void WriteTest()
    {
        // Arrange
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream);
        // Act
        _converter.Write(writer, new CsfValue("hello", "extra"), _options!);
        writer.Flush();
        stream.Position = 0;

        // Assert
        using StreamReader reader = new(stream);
        string json = reader.ReadToEnd();
        Assert.AreEqual("""{"value":"hello","extra":"extra"}""", json);
    }
}