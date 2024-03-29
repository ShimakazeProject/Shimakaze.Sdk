﻿using System.Text;
using System.Text.Json;

using Shimakaze.Sdk.Csf.Json.Converter.V1;

namespace Shimakaze.Sdk.Csf.Json.Tests.Converter.V1;

[TestClass]
public class CsfLanguageJsonConverterTests
{
    private readonly CsfLanguageJsonConverter _converter = new();

    private JsonSerializerOptions? _options;

    [TestMethod]
    public void ReadShouldReturnIntValueWhenReaderHasNumberToken()
    {
        // Arrange
        var reader = new Utf8JsonReader("42"u8);
        reader.Read(); // Move to the start object token

        // Act
        var result = _converter.Read(ref reader, typeof(int), _options!);

        // Assert
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ReadShouldReturnIntValueWhenReaderHasStringTokenAndValidLanguageCode()
    {
        // Arrange
        var reader = new Utf8JsonReader("\"fr\""u8);
        reader.Read(); // Move to the start object token

        // Act
        var result = _converter.Read(ref reader, typeof(int), _options!);

        // Assert
        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void ReadShouldReturnIntValueWhenReaderHasStringTokenAndValidLanguageCode1()
    {
        var arr = new[]
        {
            "\"en_US\"",
            "\"en_UK\"",
            "\"de\"",
            "\"fr\"",
            "\"es\"",
            "\"it\"",
            "\"jp\"",
            "\"Jabberwockie\"",
            "\"kr\"",
            "\"zh\"",
        };
        for (int i = 0; i < arr.Length; i++)
        {
            // Arrange
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(arr[i]));
            reader.Read(); // Move to the start object token

            // Act
            var result = _converter.Read(ref reader, typeof(int), _options!);

            // Assert
            Assert.AreEqual(i, result);
        }
    }

    [TestInitialize]
    public void Startup()
    {
        _options ??= new();
        foreach (var item in CsfJsonSerializerOptions.Converters)
            _options.Converters.Add(item);
    }

    [TestMethod]
    public void WriteShouldWriteNumberValueWhenValueIsUnknownLanguageCode()
    {
        // Arrange
        var value = 10;
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        // Act
        _converter.Write(writer, value, _options!);
        writer.Flush();
        stream.Position = 0;

        // Assert
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        Assert.AreEqual("10", json);
    }

    [TestMethod]
    public void WriteShouldWriteStringValueWhenValueIsKnownLanguageCode()
    {
        // Arrange
        var value = 6;
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        // Act
        _converter.Write(writer, value, _options!);
        writer.Flush();
        stream.Position = 0;

        // Assert
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        Assert.AreEqual("\"jp\"", json);
    }

    [TestMethod]
    public void WriteShouldWriteStringValueWhenValueIsKnownLanguageCode1()
    {
        var arr = new[]
        {
            "\"en_US\"",
            "\"en_UK\"",
            "\"de\"",
            "\"fr\"",
            "\"es\"",
            "\"it\"",
            "\"jp\"",
            "\"Jabberwockie\"",
            "\"kr\"",
            "\"zh\"",
        };

        for (int i = 0; i < arr.Length; i++)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            // Act
            _converter.Write(writer, i, _options!);
            writer.Flush();
            stream.Position = 0;

            // Assert
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            Assert.AreEqual(arr[i], json);
        }
    }
}