using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Tests.Data.Csf;

[TestClass]
public class CsfMetadataTests
{
    [TestMethod]
    public void Test()
    {
        CsfMetadata metadata = new(0, 0);
        Assert.AreEqual(0, metadata.Unknown);
        metadata.Unknown = 0x5c9b98ce;
        Assert.AreEqual(0x5c9b98ce, metadata.Unknown);
    }

    [TestMethod]
    public void Test1()
    {
        CsfMetadata metadata = new(0, 0, 0, 0, 0, 0);
        Assert.AreEqual(0, metadata.Identifier);
        Assert.AreEqual(0, metadata.Version);
        Assert.AreEqual(0, metadata.LabelCount);
        Assert.AreEqual(0, metadata.StringCount);
        Assert.AreEqual(0, metadata.Unknown);
        Assert.AreEqual(0, metadata.Language);
    }
}
