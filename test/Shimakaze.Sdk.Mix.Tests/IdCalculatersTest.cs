
namespace Shimakaze.Sdk.Mix.Tests;

[TestClass]
public class IdCalculatersTest
{
    private const string CsfFile = "ra2md.csf";
    private const string LxdFile = "local mix database.dat";
    private const uint Ra2mdCsf = 3179499641;

    [TestMethod]
    public void TSIdCalculaterTest()
    {
        Assert.AreEqual(Ra2mdCsf, IdCalculaters.TSIdCalculater(CsfFile));
        Assert.AreEqual(Constants.LXD_TS_ID, IdCalculaters.TSIdCalculater(LxdFile));
    }

    [TestMethod]
    public void TDIdCalculaterTest()
    {
        Assert.AreEqual(Constants.LXD_TD_ID, IdCalculaters.TDIdCalculater(LxdFile));
    }
}