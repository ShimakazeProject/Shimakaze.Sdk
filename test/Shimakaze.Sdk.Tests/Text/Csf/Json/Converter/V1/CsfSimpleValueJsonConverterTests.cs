using System.Text.Json;

using Shimakaze.Sdk.Text.Csf.Json.Converter.V1;

namespace Shimakaze.Sdk.Tests.Text.Csf.Json.Converter.V1;

[TestClass]
public class CsfSimpleValueJsonConverterTests
{
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
        CsfSimpleValueJsonConverter converter = new();

        Assert.ThrowsException<JsonException>(() =>
        {
            var reader = new Utf8JsonReader("""{"hello":null}"""u8);
            reader.Read();
            converter.Read(ref reader, typeof(string), _options!);
        });
    }
}