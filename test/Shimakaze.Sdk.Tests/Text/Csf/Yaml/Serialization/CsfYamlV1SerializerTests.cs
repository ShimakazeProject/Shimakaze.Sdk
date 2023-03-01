using Shimakaze.Sdk.Data.Csf;
using Shimakaze.Sdk.Data.Csf.Serialization;
using Shimakaze.Sdk.Text.Csf.Yaml.Serialization;

namespace Shimakaze.Sdk.Tests.Text.Csf.Yaml.Serialization;

[TestClass()]
public class CsfYamlV1SerializerTests
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";
    private const string InputFile2 = "ra2md.v1.csf.yaml";
    private const string OutputPath = "Out";

    public CsfYamlV1SerializerTests()
    {
        if (!Directory.Exists(OutputPath))
        {
            Directory.CreateDirectory(OutputPath);
        }
    }

    [TestMethod()]
    public void DeserializeTest()
    {
        using StreamReader sr = File.OpenText(Path.Combine(Assets, InputFile2));
        CsfDocument doc = CsfYamlV1Serializer.Deserialize(sr);
        using Stream output = File.Create(Path.Combine(OutputPath, "DeserializeTest.csf"));
        using Stream output1 = File.Create(Path.Combine(OutputPath, "DeserializeTest.csf.yml"));
        CsfSerializer.Serialize(output, doc);
        CsfYamlV1Serializer.Serialize(output1, doc);
    }

    [TestMethod()]
    public void DeserializeTest1()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile2));
        CsfDocument doc = CsfYamlV1Serializer.Deserialize(stream);
        using Stream output = File.Create(Path.Combine(OutputPath, "DeserializeTest1.csf"));
        using Stream output1 = File.Create(Path.Combine(OutputPath, "DeserializeTest1.csf.yml"));
        CsfSerializer.Serialize(output, doc);
        CsfYamlV1Serializer.Serialize(output1, doc);
    }

    [TestMethod()]
    public void SerializeTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        CsfDocument document = CsfSerializer.Deserialize(stream);

        if (!Directory.Exists(OutputPath))
        {
            Directory.CreateDirectory(OutputPath);
        }

        string path = Path.Combine(OutputPath, "SerializeTest.csf.yaml");
        using StreamWriter sw = File.CreateText(path);
        CsfYamlV1Serializer.Serialize(sw, document);
    }

    [TestMethod()]
    public void SerializeTest1()
    {
        using Stream stream1 = File.OpenRead(Path.Combine(Assets, InputFile));
        CsfDocument document = CsfSerializer.Deserialize(stream1);

        if (!Directory.Exists(OutputPath))
        {
            Directory.CreateDirectory(OutputPath);
        }

        string path = Path.Combine(OutputPath, "SerializeTest1.csf.yaml");
        using Stream stream = File.Create(path);
        CsfYamlV1Serializer.Serialize(stream, document);
    }
}
