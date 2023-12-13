namespace Shimakaze.Sdk.Ini.Tests;

[TestClass()]
public class IniTokenToolsTests
{
    [TestMethod()]
    public void WhiteTest()
    {
        Assert.AreEqual(-1, IniTokenTools.EOF.Token);
        Assert.AreEqual('\r', IniTokenTools.CR.Token);
        Assert.AreEqual('\n', IniTokenTools.LF.Token);
        Assert.AreEqual(' ', IniTokenTools.SPACE.Token);
        Assert.AreEqual('\t', IniTokenTools.TAB.Token);
    }

    [TestMethod()]
    public void SignTest()
    {
        Assert.AreEqual(';', IniTokenTools.SEMI.Token);
        Assert.AreEqual('=', IniTokenTools.EQ.Token);
        Assert.AreEqual('[', IniTokenTools.BeginBracket.Token);
        Assert.AreEqual(']', IniTokenTools.EndBracket.Token);
    }

    [TestMethod()]
    public void StringTest()
    {
        string value = Guid.NewGuid().ToString();
        var token = IniTokenTools.Value(value);
        Assert.AreEqual(1, token.Token);
        Assert.AreEqual(value, token.Value);
    }
}