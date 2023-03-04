using Shimakaze.Sdk.Data.Csf;
using Shimakaze.Sdk.Data.Csf.Serialization;
using Shimakaze.Sdk.Text.Csf.Yaml.Serialization;

namespace Shimakaze.Sdk.Tests.Text.Csf.Yaml.Serialization;

[TestClass]
public class CsfYamlV1SerializerTests
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";
    private const string InputFile2 = "ra2md.v1.csf.yaml";
    private const string OutputPath = "Out";
    private const string OutputDeserializeCsfFile1 = "DeserializeTest1.v1.csf";
    private const string OutputDeserializeYamlFile1 = "DeserializeTest1.v1.csf.yml";
    private const string OutputDeserializeCsfFile2 = "DeserializeTest2.v1.csf";
    private const string OutputDeserializeYamlFile2 = "DeserializeTest2.v1.csf.yml";
    private const string OutputSerializeFile1 = "SerializeTest1.v1.csf.yml";
    private const string OutputSerializeFile2 = "SerializeTest2.v1.csf.yml";

    [TestInitialize]
    public void Startup()
    {
        if (!Directory.Exists(OutputPath))
        {
            Directory.CreateDirectory(OutputPath);
        }
    }

    [TestMethod]
    public void DeserializeTest1()
    {
        using StreamReader sr = File.OpenText(Path.Combine(Assets, InputFile2));
        CsfDocument doc = CsfYamlV1Serializer.Deserialize(sr);
        using Stream output = File.Create(Path.Combine(OutputPath, OutputDeserializeCsfFile1));
        using Stream output1 = File.Create(Path.Combine(OutputPath, OutputDeserializeYamlFile1));
        CsfSerializer.Serialize(output, doc);
        CsfYamlV1Serializer.Serialize(output1, doc);
    }

    [TestMethod]
    public void DeserializeTest2()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile2));
        CsfDocument doc = CsfYamlV1Serializer.Deserialize(stream);
        using Stream output = File.Create(Path.Combine(OutputPath, OutputDeserializeCsfFile2));
        using Stream output1 = File.Create(Path.Combine(OutputPath, OutputDeserializeYamlFile2));
        CsfSerializer.Serialize(output, doc);
        CsfYamlV1Serializer.Serialize(output1, doc);
    }

    [TestMethod]
    public void SerializeTest1()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        CsfDocument document = CsfSerializer.Deserialize(stream);

        string path = Path.Combine(OutputPath, OutputSerializeFile1);
        using StreamWriter sw = File.CreateText(path);
        CsfYamlV1Serializer.Serialize(sw, document);
    }

    [TestMethod]
    public void SerializeTest2()
    {
        using Stream stream1 = File.OpenRead(Path.Combine(Assets, InputFile));
        CsfDocument document = CsfSerializer.Deserialize(stream1);

        string path = Path.Combine(OutputPath, OutputSerializeFile2);
        using Stream stream = File.Create(path);
        CsfYamlV1Serializer.Serialize(stream, document);
    }
}
