using System.Text.Json;

using Shimakaze.Sdk.Csf.Json.Converter.V1;

namespace Shimakaze.Sdk.Csf.Json.Tests.Converter.V1;

[TestClass]
public class CsfMetadataJsonConverterTests
{
    private readonly CsfMetadataJsonConverter _converter = new();

    private JsonSerializerOptions? _options;

    [TestMethod]
    public void ReadTest()
    {
        var reader = new Utf8JsonReader("""{"hello":null}"""u8);
        reader.Read();
        _converter.Read(ref reader, typeof(int), _options!);
    }

    [TestInitialize]
    public void Startup()
    {
        _options ??= new();
        foreach (var item in CsfJsonSerializerOptions.Converters)
            _options.Converters.Add(item);
    }
}