using Shimakaze.Sdk.Data.Csf;

namespace Shimakaze.Sdk.Tests.Data.Csf;

[TestClass]
public class CsfThrowHelperTests
{
    [TestMethod]
    public void Test()
    {
        Assert.AreEqual(CsfConstants.CsfFlagRaw, CsfThrowHelper.IsCsfFile(CsfConstants.CsfFlagRaw));
        try
        {
            CsfThrowHelper.IsCsfFile(CsfConstants.LblFlagRaw);
        }
        catch (FormatException e)
        {
            Assert.AreEqual("It's not CSF File Flag.", e.Message);
        }

        Assert.AreEqual(CsfConstants.LblFlagRaw, CsfThrowHelper.IsLabel(CsfConstants.LblFlagRaw, () => new[] { string.Empty }));
        try
        {
            CsfThrowHelper.IsLabel(CsfConstants.CsfFlagRaw, () => new object[] { "0", 1 });
        }
        catch (FormatException e)
        {
            Assert.AreEqual("It's not CSF Label Flag. #0 at 0x00000001.", e.Message);
        }

        Assert.AreEqual(CsfConstants.StrFlagRaw, CsfThrowHelper.IsStringOrExtraString(CsfConstants.StrFlagRaw, () => new[] { string.Empty }));
        Assert.AreEqual(CsfConstants.StrwFlgRaw, CsfThrowHelper.IsStringOrExtraString(CsfConstants.StrwFlgRaw, () => new[] { string.Empty }));
        try
        {
            CsfThrowHelper.IsStringOrExtraString(CsfConstants.CsfFlagRaw, () => new object[] { "0", "1", 1 });
        }
        catch (FormatException e)
        {
            Assert.AreEqual("It's not CSF String Flag #0:1 at 0x00000001.", e.Message);
        }
    }
}