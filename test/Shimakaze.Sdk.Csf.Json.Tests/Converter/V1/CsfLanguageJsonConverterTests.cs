using System.Text;
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
        Utf8JsonReader reader = new("42"u8);
        reader.Read(); // Move to the start object token

        // Act
        int result = _converter.Read(ref reader, typeof(int), _options!);

        // Assert
        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void ReadShouldReturnIntValueWhenReaderHasStringTokenAndValidLanguageCode()
    {
        // Arrange
        Utf8JsonReader reader = new("\"fr\""u8);
        reader.Read(); // Move to the start object token

        // Act
        int result = _converter.Read(ref reader, typeof(int), _options!);

        // Assert
        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public void ReadShouldReturnIntValueWhenReaderHasStringTokenAndValidLanguageCode1()
    {
        string[] arr = new[]
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
            Utf8JsonReader reader = new(Encoding.UTF8.GetBytes(arr[i]));
            reader.Read(); // Move to the start object token

            // Act
            int result = _converter.Read(ref reader, typeof(int), _options!);

            // Assert
            Assert.AreEqual(i, result);
        }
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
    public void WriteShouldWriteNumberValueWhenValueIsUnknownLanguageCode()
    {
        // Arrange
        int value = 10;
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream);
        // Act
        _converter.Write(writer, value, _options!);
        writer.Flush();
        stream.Position = 0;

        // Assert
        using StreamReader reader = new(stream);
        string json = reader.ReadToEnd();
        Assert.AreEqual("10", json);
    }

    [TestMethod]
    public void WriteShouldWriteStringValueWhenValueIsKnownLanguageCode()
    {
        // Arrange
        int value = 6;
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream);
        // Act
        _converter.Write(writer, value, _options!);
        writer.Flush();
        stream.Position = 0;

        // Assert
        using StreamReader reader = new(stream);
        string json = reader.ReadToEnd();
        Assert.AreEqual("\"jp\"", json);
    }

    [TestMethod]
    public void WriteShouldWriteStringValueWhenValueIsKnownLanguageCode1()
    {
        string[] arr = new[]
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
            using MemoryStream stream = new();
            using Utf8JsonWriter writer = new(stream);
            // Act
            _converter.Write(writer, i, _options!);
            writer.Flush();
            stream.Position = 0;

            // Assert
            using StreamReader reader = new(stream);
            string json = reader.ReadToEnd();
            Assert.AreEqual(arr[i], json);
        }
    }
}