namespace Shimakaze.Sdk.Ini.Ares.Tests;

[TestClass()]
public class AresIniTokenToolsTests
{
    [TestMethod()]
    public void WhiteTest()
    {
        Assert.AreEqual(-1, AresIniTokenTools.EOF.Token);
        Assert.AreEqual('\r', AresIniTokenTools.CR.Token);
        Assert.AreEqual('\n', AresIniTokenTools.LF.Token);
        Assert.AreEqual(' ', AresIniTokenTools.SPACE.Token);
        Assert.AreEqual('\t', AresIniTokenTools.TAB.Token);
    }

    [TestMethod()]
    public void SignTest()
    {
        Assert.AreEqual(';', AresIniTokenTools.SEMI.Token);
        Assert.AreEqual('=', AresIniTokenTools.EQ.Token);
        Assert.AreEqual('[', AresIniTokenTools.BeginBracket.Token);
        Assert.AreEqual(']', AresIniTokenTools.EndBracket.Token);
    }

    [TestMethod()]
    public void StringTest()
    {
        string value = Guid.NewGuid().ToString();
        var token = AresIniTokenTools.Value(value);
        Assert.AreEqual(1, token.Token);
        Assert.AreEqual(value, token.Value);
    }
}