using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Tests.Data.Csf;

[TestClass]
public class CsfMetadataTests
{
    [TestMethod]
    public void Test()
    {
        CsfMetadata metadata = default;
        Assert.AreEqual(0, metadata.Unknown);
        metadata.Unknown = 0x5c9b98ce;
        Assert.AreEqual(0x5c9b98ce, metadata.Unknown);
    }
}
