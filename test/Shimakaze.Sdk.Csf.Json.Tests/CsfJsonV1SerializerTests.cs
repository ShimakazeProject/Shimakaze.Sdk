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
        using Stream input = File.OpenRead(Path.Combine(Assets, InputFile2));
        using Stream output1 = File.Create(Path.Combine(OutputPath, OutputTestCsfFile));
        using Stream output2 = File.Create(Path.Combine(OutputPath, OutputTestJsonFile));
        CsfDocument doc = await CsfJsonV1Reader.ReadAsync(input);
        Assert.IsNotNull(doc);
        CsfWriter.Write(output1, doc);
        await CsfJsonV1Writer.WriteAsync(output2, doc);
    }

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }
}