using Shimakaze.Sdk.IO.Csf;

namespace Shimakaze.Sdk.Csf.Json.Serialization;

[TestClass]
public class CsfJsonV1SerializerTests
{
    private const string Assets = "Assets";
    private const string InputFile2 = "ra2md.v1.csf.json";
    private const string OutputPath = "Out";
    private const string OutputTestCsfFile = "Test.v1.csf";
    private const string OutputTestJsonFile = "Test.v1.csf.json";
    private const string OutputTestAsyncCsfFile = "TestAsync.v1.csf";
    private const string OutputTestAsyncJsonFile = "TestAsync.v1.csf.json";

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public void DeserializeTest()
    {
        using Stream input = File.OpenRead(Path.Combine(Assets, InputFile2));
        using Stream output1 = File.Create(Path.Combine(OutputPath, OutputTestCsfFile));
        using Stream output2 = File.Create(Path.Combine(OutputPath, OutputTestJsonFile));
        using CsfJsonV1Deserializer deserializer = new(input);
        using CsfWriter writer = new(output1);
        using CsfJsonV1Serializer serializer2 = new(output2);
        CsfDocument doc = deserializer.Deserialize();
        Assert.IsNotNull(doc);
        writer.Write(doc);
        serializer2.Serialize(doc);
    }

    [TestMethod]
    public async Task DeserializeAsyncTest()
    {
        await using Stream input = File.OpenRead(Path.Combine(Assets, InputFile2));
        await using Stream output1 = File.Create(Path.Combine(OutputPath, OutputTestAsyncCsfFile));
        await using Stream output2 = File.Create(Path.Combine(OutputPath, OutputTestAsyncJsonFile));
        await using CsfJsonV1Deserializer deserializer = new(input);
        await using CsfWriter writer = new(output1);
        await using CsfJsonV1Serializer serializer2 = new(output2);
        CsfDocument doc = await deserializer.DeserializeAsync();
        Assert.IsNotNull(doc);
        writer.Write(doc);
        await serializer2.SerializeAsync(doc);
    }
}