using Shimakaze.Sdk.Csf;

namespace Shimakaze.Sdk.IO.Csf.Xml;

[TestClass]
public class CsfXmlV1WriterTests
{
    private const string Assets = "Assets";
    private const string InputCsfFile = "ra2md.csf";
    private const string InputXmlFile = "ra2md.v1.csf.xml";
    private const string OutputDeserializeCsfFile = "DeserializeTest.v1.csf";
    private const string OutputDeserializeXmlFile = "DeserializeTest.v1.csf.xml";
    private const string OutputPath = "Out";
    private const string OutputSerializeFile = "SerializeTest.v1.csf.xml";

    [TestMethod]
    public async Task DeserializeTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputXmlFile));
        using Stream xmlout = File.Create(Path.Combine(OutputPath, OutputDeserializeXmlFile));
        using Stream csfout = File.Create(Path.Combine(OutputPath, OutputDeserializeCsfFile));
        using CsfXmlV1Reader deserializer = new(stream);
        using CsfXmlV1Writer xmlSerializer = new(xmlout);
        using CsfWriter writer = new(csfout);
        CsfDocument doc = await deserializer.ReadAsync();
        Assert.IsNotNull(doc);
        await writer.WriteAsync(doc);
        await xmlSerializer.WriteAsync(doc);
    }

    [TestMethod]
    public async Task SerializeTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputCsfFile));
        using Stream output = File.Create(Path.Combine(OutputPath, OutputSerializeFile));
        using CsfReader reader = new(stream);
        using CsfXmlV1Writer serializer = new(output);
        CsfDocument document = await reader.ReadAsync();
        await serializer.WriteAsync(document);
    }

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }
}