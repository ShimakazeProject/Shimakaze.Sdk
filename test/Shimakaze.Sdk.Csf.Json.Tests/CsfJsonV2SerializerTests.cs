using Shimakaze.Sdk.Csf.IO;
using Shimakaze.Sdk.Csf.Json.IO;

namespace Shimakaze.Sdk.Csf.Json.Tests;

[TestClass]
public class CsfJsonV2WriterTests
{
    private const string Assets = "Assets";
    private const string InputFile2 = "ra2md.v2.csf.json";
    private const string OutputPath = "Out";
    private const string OutputTestCsfFile = "Test.v2.csf";
    private const string OutputTestJsonFile = "Test.v2.csf.json";

    [TestMethod]
    public async Task DeserializeAsyncTest()
    {
        using Stream input = File.OpenRead(Path.Combine(Assets, InputFile2));
        using Stream output1 = File.Create(Path.Combine(OutputPath, OutputTestCsfFile));
        using CsfWriter writer = new(output1);
        using Stream output2 = File.Create(Path.Combine(OutputPath, OutputTestJsonFile));
        var doc = await CsfJsonV2.ReadAllDataAsync(input);
        Assert.IsNotNull(doc);
        writer.WriteAllData(doc);
        await CsfJsonV2.WriteAllDataAsync(output2, doc);
    }

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }
}
