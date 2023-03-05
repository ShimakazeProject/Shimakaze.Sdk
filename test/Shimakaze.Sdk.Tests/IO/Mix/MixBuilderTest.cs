using Shimakaze.Sdk.Data.Mix;
using Shimakaze.Sdk.IO.Mix;

namespace Shimakaze.Sdk.Tests.IO.Mix;

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
        await new MixBuilder()
            .AddFile(new(Path.Combine(Assets, CsfFile)))
            .SetIdCaculater(IdCalculaters.TSIdCalculater)
            .BuildAsync(fs)
            .ConfigureAwait(false);
        await fs.FlushAsync().ConfigureAwait(false);
    }
}