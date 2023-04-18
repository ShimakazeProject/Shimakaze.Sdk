using Shimakaze.Sdk.IO.Mix;

namespace Shimakaze.Sdk.Mix;

[TestClass]
public class MixBuilderTest
{
    private const string Assets = "Assets";
    private const string MixFile = "MixBuilderTest.mix";
    private const string CsfFile = "ra2md.csf";

    private const string OutputPath = "Out";

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public async Task Test()
    {
        await using var fs = File.Create(Path.Combine(OutputPath, MixFile));
        FileInfo fileInfo = new(Path.Combine(Assets, CsfFile));
        MixBuilder builder = new();
        builder.AddFile(fileInfo);
        Assert.AreEqual(1, builder.FileCount);

        builder.RemoveFile(fileInfo);
        Assert.AreEqual(0, builder.FileCount);

        builder.AddFile(fileInfo);
        Assert.AreEqual(1, builder.FileCount);

        await Assert.ThrowsExceptionAsync<NullIdCalculaterException>(async () => await builder.BuildAsync(fs).ConfigureAwait(false));
        builder.SetIdCaculater(IdCalculaters.TSIdCalculater);

        await builder.BuildAsync(fs).ConfigureAwait(false);

        await fs.FlushAsync().ConfigureAwait(false);
    }
}