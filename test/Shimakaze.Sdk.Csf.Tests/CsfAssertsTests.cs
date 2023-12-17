namespace Shimakaze.Sdk.Csf.Tests;

[TestClass]
public class CsfAssertsTests
{
    [TestMethod]
    public void Test()
    {
        Assert.AreEqual(CsfConstants.CsfFlagRaw, CsfAsserts.IsCsfFile(CsfConstants.CsfFlagRaw));
        try
        {
            CsfAsserts.IsCsfFile(CsfConstants.LblFlagRaw);
        }
        catch (FormatException e)
        {
            Assert.AreEqual("It's not CSF File Flag.", e.Message);
        }

        Assert.AreEqual(CsfConstants.LblFlagRaw, CsfAsserts.IsLabel(CsfConstants.LblFlagRaw, () => new[] { string.Empty }));
        try
        {
            CsfAsserts.IsLabel(CsfConstants.CsfFlagRaw, () => new object[] { "0", 1 });
        }
        catch (FormatException e)
        {
            Assert.AreEqual("It's not CSF Label Flag. #0 at 0x00000001.", e.Message);
        }

        Assert.AreEqual(CsfConstants.StrFlagRaw, CsfAsserts.IsStringOrExtraString(CsfConstants.StrFlagRaw, () => new[] { string.Empty }));
        Assert.AreEqual(CsfConstants.StrwFlgRaw, CsfAsserts.IsStringOrExtraString(CsfConstants.StrwFlgRaw, () => new[] { string.Empty }));
        try
        {
            CsfAsserts.IsStringOrExtraString(CsfConstants.CsfFlagRaw, () => new object[] { "0", "1", 1 });
        }
        catch (FormatException e)
        {
            Assert.AreEqual("It's not CSF String Flag #0:1 at 0x00000001.", e.Message);
        }
    }
}