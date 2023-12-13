namespace Shimakaze.Sdk.Ini.Tests;

[TestClass()]
public class IniTokenReaderTests
{
    [TestMethod()]
    public void Test()
    {
        const string ini = "[Section]\r\nKey=Value\r\n";
        Stack<IIniToken> tokens = new();
        tokens.Push(IniTokenTools.EOF);
        tokens.Push(IniTokenTools.LF);
        tokens.Push(IniTokenTools.CR);
        tokens.Push(IniTokenTools.Value("Value"));
        tokens.Push(IniTokenTools.EQ);
        tokens.Push(IniTokenTools.Value("Key"));
        tokens.Push(IniTokenTools.LF);
        tokens.Push(IniTokenTools.CR);
        tokens.Push(IniTokenTools.EndBracket);
        tokens.Push(IniTokenTools.Value("Section"));
        tokens.Push(IniTokenTools.BeginBracket);

        using StringReader sr = new(ini);
        using IniTokenReader reader = new(sr);
        foreach (var item in reader)
        {
            Console.WriteLine(item);
            Assert.AreEqual(tokens.Pop(), item);
        }
    }
}