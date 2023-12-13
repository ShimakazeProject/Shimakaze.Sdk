namespace Shimakaze.Sdk.Ini.Ares.Tests;

[TestClass()]
public class AresIniTokenReaderTests
{
    [TestMethod()]
    public void Test()
    {
        const string ini = "[Section]\r\nKey=Value\r\n";
        Stack<IIniToken> tokens = new();
        tokens.Push(AresIniTokenTools.EOF);
        tokens.Push(AresIniTokenTools.LF);
        tokens.Push(AresIniTokenTools.CR);
        tokens.Push(AresIniTokenTools.Value("Value"));
        tokens.Push(AresIniTokenTools.EQ);
        tokens.Push(AresIniTokenTools.Value("Key"));
        tokens.Push(AresIniTokenTools.LF);
        tokens.Push(AresIniTokenTools.CR);
        tokens.Push(AresIniTokenTools.EndBracket);
        tokens.Push(AresIniTokenTools.Value("Section"));
        tokens.Push(AresIniTokenTools.BeginBracket);

        using StringReader sr = new(ini);
        using AresIniTokenReader reader = new(sr);
        foreach (var item in reader)
        {
            Console.WriteLine(item);
            Assert.AreEqual(tokens.Pop(), item);
        }
    }
}