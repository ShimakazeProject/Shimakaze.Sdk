using Shimakaze.Sdk.Csf;

namespace Shimakaze.Sdk.IO.Csf.Yaml.Tests;

[TestClass]
public class CsfYamlV1WriterTests
{
    private const string Assets = "Assets";
    private const string InputCsfFile = "ra2md.csf";
    private const string InputYmlFile = "ra2md.v1.csf.yaml";
    private const string OutputDeserializeCsfFile = "DeserializeTest.v1.csf";
    private const string OutputDeserializeYamlFile = "DeserializeTest.v1.csf.yml";
    private const string OutputPath = "Out";
    private const string OutputSerializeFile = "SerializeTest.v1.csf.yml";

    [TestMethod]
    public async Task DeserializeTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputYmlFile));
        using Stream csfout = File.Create(Path.Combine(OutputPath, OutputDeserializeCsfFile));
        using Stream ymlout = File.Create(Path.Combine(OutputPath, OutputDeserializeYamlFile));
        using CsfYamlV1Reader deserializer = new(stream);
        using CsfWriter writer = new(csfout);
        using CsfYamlV1Writer yamlV1Writer = new(ymlout);
        CsfDocument doc = await deserializer.ReadAsync();
        await writer.WriteAsync(doc);
        await yamlV1Writer.WriteAsync(doc);
    }

    [TestMethod]
    public async Task SerializeTestAsync()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputCsfFile));
        using Stream output = File.Create(Path.Combine(OutputPath, OutputSerializeFile));
        using CsfReader reader = new(stream);
        using CsfYamlV1Writer serializer = new(output);
        CsfDocument document = await reader.ReadAsync();
        await serializer.WriteAsync(document);
    }

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }
}