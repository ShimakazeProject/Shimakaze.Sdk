namespace Shimakaze.Sdk.Csf.Tests;

[TestClass]
public class CsfAssertsTests
{
    [TestMethod]
    public void IsCsfFileTest()
    {
        Assert.AreEqual(CsfConstants.CsfFlagRaw, CsfAsserts.IsCsfFile(CsfConstants.CsfFlagRaw));
        Assert.ThrowsException<FormatException>(static () => CsfAsserts.IsCsfFile(-1), "It's not CSF File Flag.");
    }

    [TestMethod]
    public void IsLabelTest()
    {
        Assert.AreEqual(CsfConstants.LblFlagRaw, CsfAsserts.IsLabel(CsfConstants.LblFlagRaw, static () => [string.Empty]));
        Assert.ThrowsException<FormatException>(static () => CsfAsserts.IsLabel(-1, static () => [1]), "It's not CSF Label Flag. 0x00000001.");
    }

    [TestMethod]
    public void IsStringTest()
    {
        Assert.AreEqual(CsfConstants.StrFlagRaw, CsfAsserts.IsStringOrExtraString(CsfConstants.StrFlagRaw, static () => [string.Empty]));
        Assert.ThrowsException<FormatException>(static () => CsfAsserts.IsStringOrExtraString(-1, static () => [1]), "It's not CSF String Flag 0x00000001.");
    }

    [TestMethod]
    public void IsExtraStringTest()
    {
        Assert.AreEqual(CsfConstants.StrwFlgRaw, CsfAsserts.IsStringOrExtraString(CsfConstants.StrwFlgRaw, static () => [string.Empty]));
        Assert.ThrowsException<FormatException>(static () => CsfAsserts.IsStringOrExtraString(-1, static () => [1]), "It's not CSF String Flag 0x00000001.");
    }
}
