using Shimakaze.Sdk.Csf;

namespace Shimakaze.Sdk.Csf.Json.Tests;

[TestClass]
public class CsfJsonV1WriterTests
{
    private const string Assets = "Assets";
    private const string InputFile2 = "ra2md.v1.csf.json";
    private const string OutputPath = "Out";
    private const string OutputTestCsfFile = "Test.v1.csf";
    private const string OutputTestJsonFile = "Test.v1.csf.json";

    [TestMethod]
    public async Task DeserializeAsyncTest()
    {
        await using Stream input = File.OpenRead(Path.Combine(Assets, InputFile2));
        await using Stream output1 = File.Create(Path.Combine(OutputPath, OutputTestCsfFile));
        await using Stream output2 = File.Create(Path.Combine(OutputPath, OutputTestJsonFile));
        await using CsfJsonV1Reader deserializer = new(input);
        await using CsfWriter writer = new(output1);
        await using CsfJsonV1Writer serializer2 = new(output2);
        CsfDocument doc = await deserializer.ReadAsync();
        Assert.IsNotNull(doc);
        await writer.WriteAsync(doc);
        await serializer2.WriteAsync(doc);
    }

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }
}