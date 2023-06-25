﻿using System.Text;
using System.Text.Json;

namespace Shimakaze.Sdk.Csf.Json.Converter.V1;

[TestClass]
public class CsfAdvancedValueJsonConverterTests
{
    private readonly CsfAdvancedValueJsonConverter _converter = new();
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
        var json = """{"value":"Hello","extra":"World"}"""u8;
        var reader = new Utf8JsonReader(json);
        reader.Read(); // Move to the start object token

        // Act
        var result = _converter.Read(ref reader, typeof(CsfValue), _options!);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(CsfValue));
        Assert.AreEqual("Hello", result.Value);
        Assert.AreEqual("World", result.ExtraValue);
    }

    // This is the test method for Read method
    [TestMethod]
    public void Read_ShouldReturnCorrectCsfValue1()
    {
        // Arrange
        var json = """{"value":"Hello","extra":"World"}"""u8;
        var reader = new Utf8JsonReader(json);
        reader.Read(); // Move to the start object token

        // Act
        var result = _converter.Read(ref reader, typeof(CsfValue), _options!);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(CsfValue));
        Assert.AreEqual("Hello", result.Value);
    }

    // This is the test method for Write method
    [TestMethod]
    public void Write_ShouldWriteCorrectJson()
    {
        // Arrange
        var value = new CsfValue("Hello", "World");
        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms);

        // Act
        _converter.Write(writer, value, _options!);
        // Flush the writer to get the json bytes
        writer.Flush();
        var json = Encoding.UTF8.GetString(ms.ToArray());

        // Assert
        Assert.AreEqual("""{"value":"Hello","extra":"World"}""", json);
    }
}
