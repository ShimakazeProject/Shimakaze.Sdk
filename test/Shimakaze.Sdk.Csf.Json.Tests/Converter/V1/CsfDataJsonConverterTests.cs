﻿using System.Text;
using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json.Converter.V1;

[TestClass]
public class CsfDataJsonConverterTests
{
    private readonly CsfDataJsonConverter _converter = new();
    private JsonSerializerOptions? _options;

    [TestInitialize]
    public void Startup()
    {
        _options ??= new();
        foreach (var item in CsfJsonSerializerOptions.Converters)
            _options.Converters.Add(item);
    }

    // Generated by Sydney

    // This is the test method for Read method
    [TestMethod]
    public void Read_ShouldReturnCorrectCsfValue()
    {
        // Arrange
        var json = """{"label":"Hello","values":["The","World"]}"""u8;
        var reader = new Utf8JsonReader(json);
        reader.Read(); // Move to the start object token

        // Act
        var result = _converter.Read(ref reader, typeof(CsfData), _options!);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(CsfData));
        Assert.AreEqual("Hello", result.LabelName);
        Assert.AreEqual(2, result.Values.Length);
        Assert.AreEqual("The", result.Values[0].Value);
        Assert.AreEqual("World", result.Values[1].Value);
    }

    // This is the test method for Read method
    [TestMethod]
    public void Read_ShouldReturnCorrectCsfValue1()
    {
        // Arrange
        var json = """{"label":"Hello","value":{"value":"World"},"test":null}"""u8;
        var reader = new Utf8JsonReader(json);
        reader.Read(); // Move to the start object token

        // Act
        var result = _converter.Read(ref reader, typeof(CsfData), _options!);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(CsfData));
        Assert.AreEqual("Hello", result.LabelName);
        Assert.AreEqual(1, result.Values.Length);
        Assert.AreEqual("World", result.Values[0].Value);
    }

    // This is the test method for Read method
    [TestMethod]
    public void Read_ShouldReturnCorrectCsfValue2()
    {
        // Arrange
        Assert.ThrowsException<JsonException>(() =>
        {
            var json = """{"label":"Hello","value":0}"""u8;
            var reader = new Utf8JsonReader(json);
            reader.Read(); // Move to the start object token

            var result = _converter.Read(ref reader, typeof(CsfData), _options!);
        });
    }

    // This is the test method for Write method
    [TestMethod]
    public void Write_ShouldWriteCorrectJson()
    {
        // Arrange
        var value = new CsfData("Hello", new[]
        {
            new CsfValue("The"),
            new CsfValue("World"),
        });
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);

        // Act
        _converter.Write(writer, value, _options!);
        // Flush the writer to get the json bytes
        writer.Flush();
        var json = Encoding.UTF8.GetString(ms.ToArray());

        // Assert
        Assert.AreEqual("""{"label":"Hello","values":["The","World"]}""", json);
    }
}
