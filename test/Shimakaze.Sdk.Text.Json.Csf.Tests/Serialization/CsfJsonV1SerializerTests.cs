using Shimakaze.Sdk.Data.Csf;
using Shimakaze.Sdk.Data.Csf.Serialization;

namespace Shimakaze.Sdk.Text.Json.Csf.Serialization.Tests;

[TestClass()]
public class CsfJsonV1SerializerTests
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";
    private const string InputFile2 = "ra2md.v1.csf.json";
    private const string OutputPath = "Out";

    [TestMethod()]
    public void DeserializeTest()
    {
        using StreamReader sr = File.OpenText(Path.Combine(Assets, InputFile2));
        CsfDocument? doc = CsfJsonV1Serializer.Deserialize(sr);
        Assert.IsNotNull(doc);
        using Stream output = File.Create(Path.Combine(OutputPath, "DeserializeTest.v1.csf"));
        using Stream output1 = File.Create(Path.Combine(OutputPath, "DeserializeTest.v1.csf.json"));
        CsfSerializer.Serialize(output, doc);
        CsfJsonV1Serializer.Serialize(output1, doc);
    }

    [TestMethod()]
    public void DeserializeTest1()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile2));
        CsfDocument? doc = CsfJsonV1Serializer.Deserialize(stream);
        Assert.IsNotNull(doc);
        using Stream output = File.Create(Path.Combine(OutputPath, "DeserializeTest1.v1.csf"));
        using Stream output1 = File.Create(Path.Combine(OutputPath, "DeserializeTest1.v1.csf.json"));
        CsfSerializer.Serialize(output, doc);
        CsfJsonV1Serializer.Serialize(output1, doc);
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

        string path = Path.Combine(OutputPath, "SerializeTest.v1.csf.json");
        using StreamWriter sw = File.CreateText(path);
        CsfJsonV1Serializer.Serialize(sw, document);
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

        string path = Path.Combine(OutputPath, "SerializeTest1.v1.csf.json");
        using Stream stream = File.Create(path);
        CsfJsonV1Serializer.Serialize(stream, document);
    }
}
