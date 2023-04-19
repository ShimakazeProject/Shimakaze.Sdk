using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Csf.Yaml.Serialization;
using Shimakaze.Sdk.IO.Csf.Serialization;

namespace Shimakaze.Sdk.Tests.Text.Csf.Yaml.Serialization;

[TestClass]
public class CsfYamlV1SerializerTests
{
    private const string Assets = "Assets";
    private const string InputCsfFile = "ra2md.csf";
    private const string InputYmlFile = "ra2md.v1.csf.yaml";
    private const string OutputPath = "Out";
    private const string OutputDeserializeCsfFile = "DeserializeTest.v1.csf";
    private const string OutputDeserializeYamlFile = "DeserializeTest.v1.csf.yml";
    private const string OutputSerializeFile = "SerializeTest.v1.csf.yml";

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public void DeserializeTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputYmlFile));
        using Stream csfout = File.Create(Path.Combine(OutputPath, OutputDeserializeCsfFile));
        using Stream ymlout = File.Create(Path.Combine(OutputPath, OutputDeserializeYamlFile));
        using CsfYamlV1Deserializer deserializer = new(stream);
        using CsfSerializer csfSerializer = new(csfout);
        using CsfYamlV1Serializer yamlV1Serializer = new(ymlout);
        CsfDocument doc = deserializer.Deserialize();
        csfSerializer.Serialize(doc);
        yamlV1Serializer.Serialize(doc);
    }

    [TestMethod]
    public void SerializeTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputCsfFile));
        using Stream output = File.Create(Path.Combine(OutputPath, OutputSerializeFile));
        using CsfDeserializer deserializer = new(stream);
        using CsfYamlV1Serializer serializer = new(output);
        CsfDocument document = deserializer.Deserialize();
        serializer.Serialize(document);
    }
}
