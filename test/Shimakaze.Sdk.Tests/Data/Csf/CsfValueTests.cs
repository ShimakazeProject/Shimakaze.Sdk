using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Tests.Data.Csf;

[TestClass]
public class CsfValueTests
{
    [TestMethod]
    public void Test()
    {
        CsfValue value = new()
        {
            Value = "value"
        };
        Assert.IsInstanceOfType<CsfValueExtra>(value.ToExtra());
        var extra = value.ToExtra("extra");
        Assert.AreEqual("extra", extra.ExtraValue);

        extra = new()
        {
            Value = "value"
        };
        Assert.IsInstanceOfType<CsfValue>(extra.ToNormal());
        Assert.IsNotInstanceOfType<CsfValueExtra>(extra.ToNormal());
    }
}