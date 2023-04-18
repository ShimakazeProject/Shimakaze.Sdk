namespace Shimakaze.Sdk.Csf;

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

        Assert.IsNotNull(CsfValue.Empty);
        GC.Collect();
        Assert.IsNotNull(CsfValue.Empty);
    }
}