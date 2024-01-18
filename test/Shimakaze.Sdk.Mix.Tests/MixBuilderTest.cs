
namespace Shimakaze.Sdk.Mix.Tests;

[TestClass]
public class MixBuilderTest
{
    private const string Assets = "Assets";
    private const string CsfFile = "ra2md.csf";
    private const string MixFile = "MixBuilderTest.mix";
    private const string OutputPath = "Out";

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public async Task Test()
    {
        using var fs = File.Create(Path.Combine(OutputPath, MixFile));
        FileInfo fileInfo = new(Path.Combine(Assets, CsfFile));
        MixBuilder builder = new(IdCalculaters.TSIdCalculater);
        builder.Files.Add(fileInfo);
        Assert.AreEqual(1, builder.Files.Count);

        builder.Files.Remove(fileInfo);
        Assert.AreEqual(0, builder.Files.Count);

        builder.Files.Add(fileInfo);
        Assert.AreEqual(1, builder.Files.Count);

        await builder.BuildAsync(fs).ConfigureAwait(false);

        await fs.FlushAsync().ConfigureAwait(false);
    }
}
