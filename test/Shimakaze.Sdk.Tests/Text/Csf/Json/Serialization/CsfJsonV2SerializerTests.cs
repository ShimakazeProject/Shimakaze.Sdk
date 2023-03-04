using Shimakaze.Sdk.Data.Csf;
using Shimakaze.Sdk.Data.Csf.Serialization;
using Shimakaze.Sdk.Text.Csf.Json.Serialization;

namespace Shimakaze.Sdk.Tests.Text.Csf.Json.Serialization;

[TestClass]
public class CsfJsonV2SerializerTests
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";
    private const string InputFile2 = "ra2md.v2.csf.json";
    private const string OutputPath = "Out";
    private const string OutputDeserializeCsfFile1 = "DeserializeTest1.v2.csf";
    private const string OutputDeserializeJsonFile1 = "DeserializeTest1.v2.csf.json";
    private const string OutputDeserializeCsfFile2 = "DeserializeTest2.v2.csf";
    private const string OutputDeserializeJsonFile2 = "DeserializeTest2.v2.csf.json";
    private const string OutputSerializeFile1 = "SerializeTest1.v2.csf.json";
    private const string OutputSerializeFile2 = "SerializeTest2.v2.csf.json";

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
        CsfDocument? doc = CsfJsonV2Serializer.Deserialize(sr);
        Assert.IsNotNull(doc);
        using Stream output = File.Create(Path.Combine(OutputPath, OutputDeserializeCsfFile1));
        using Stream output1 = File.Create(Path.Combine(OutputPath, OutputDeserializeJsonFile1));
        CsfSerializer.Serialize(output, doc);
        CsfJsonV2Serializer.Serialize(output1, doc);
    }

    [TestMethod]
    public void DeserializeTest2()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile2));
        CsfDocument? doc = CsfJsonV2Serializer.Deserialize(stream);
        Assert.IsNotNull(doc);
        using Stream output = File.Create(Path.Combine(OutputPath, OutputDeserializeCsfFile2));
        using Stream output1 = File.Create(Path.Combine(OutputPath, OutputDeserializeJsonFile2));
        CsfSerializer.Serialize(output, doc);
        CsfJsonV2Serializer.Serialize(output1, doc);
    }

    [TestMethod]
    public void SerializeTest1()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        CsfDocument document = CsfSerializer.Deserialize(stream);

        string path = Path.Combine(OutputPath, OutputSerializeFile1);
        using StreamWriter sw = File.CreateText(path);
        CsfJsonV2Serializer.Serialize(sw, document);
    }

    [TestMethod]
    public void SerializeTest2()
    {
        using Stream stream1 = File.OpenRead(Path.Combine(Assets, InputFile));
        CsfDocument document = CsfSerializer.Deserialize(stream1);

        string path = Path.Combine(OutputPath, OutputSerializeFile2);
        using Stream stream = File.Create(path);
        CsfJsonV2Serializer.Serialize(stream, document);
    }
}
