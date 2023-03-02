using Shimakaze.Sdk.Data.Mix;
using Shimakaze.Sdk.IO.Mix;

namespace Shimakaze.Sdk.Tests.IO.Mix;

[TestClass()]
public class IdCalculatersTest
{
    private const uint ra2md_csf = 3179499641;
    private const uint lxd = 913179935;
    private const string CsfFile = "ra2md.csf";
    private const string LxdFile = "local mix database.dat";

    [TestMethod()]
    public void TSIdCalculaterTest()
    {
        Assert.AreEqual(ra2md_csf, IdCalculaters.TSIdCalculater(CsfFile));
        Assert.AreEqual(lxd, IdCalculaters.TSIdCalculater(LxdFile));
    }
}