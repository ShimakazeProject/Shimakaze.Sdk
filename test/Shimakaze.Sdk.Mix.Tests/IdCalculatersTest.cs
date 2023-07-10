using Shimakaze.Sdk.IO.Mix;

namespace Shimakaze.Sdk.Mix;

[TestClass]
public class IdCalculatersTest
{
    private const string CsfFile = "ra2md.csf";
    private const uint Lxd = 913179935;
    private const string LxdFile = "local mix database.dat";
    private const uint Ra2mdCsf = 3179499641;

    [TestMethod]
    public void TSIdCalculaterTest()
    {
        Assert.AreEqual(Ra2mdCsf, IdCalculaters.TSIdCalculater(CsfFile));
        Assert.AreEqual(Lxd, IdCalculaters.TSIdCalculater(LxdFile));
    }
}