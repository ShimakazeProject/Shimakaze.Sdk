﻿using System.Text;
using System.Text.Json;

using Shimakaze.Sdk.Csf.Json.Converter.V1;

namespace Shimakaze.Sdk.Csf.Json.Tests.Converter.V1;

[TestClass]
public class CsfDataJsonConverterTests
{
    private readonly CsfDataJsonConverter _converter = new();
    private JsonSerializerOptions? _options;

    // This is the test method for Read method
    [TestMethod]
    public void ReadShouldReturnCorrectCsfValue()
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

    // Generated by Sydney This is the test method for Read method
    [TestMethod]
    public void ReadShouldReturnCorrectCsfValue1()
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

    [TestInitialize]
    public void Startup()
    {
        _options ??= new();
        foreach (var item in CsfJsonSerializerOptions.Converters)
            _options.Converters.Add(item);
    }

    // This is the test method for Write method
    [TestMethod]
    public void WriteShouldWriteCorrectJson()
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