using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Tests.Data.Csf;

[TestClass]
public class CsfValueTests
{
    [TestMethod]
    public void Test()
    {
        CsfValue value = new();
        value.Value = "value";
        Assert.IsInstanceOfType<CsfValueExtra>(value.ToExtra());
        var extra = value.ToExtra("extra");
        Assert.AreEqual("extra", extra.ExtraValue);
    }
}