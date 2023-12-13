namespace Shimakaze.Sdk.Ini.Tests;

[TestClass()]
public class IniTokenWriterTests
{
    [TestMethod()]
    public void WriteTest()
    {
        using MemoryStream ms = new();
        using StreamWriter sw = new(ms, leaveOpen: true);
        using IniTokenWriter writer = new(sw);
        writer.Write(IniTokenTools.BeginBracket);
        writer.Write(IniTokenTools.Value("Section"));
        writer.WriteLine(IniTokenTools.EndBracket);
        writer.Flush();

        ms.Seek(0, SeekOrigin.Begin);
        using StreamReader sr = new(ms);
        Assert.AreEqual("[Section]" + sw.NewLine, sr.ReadToEnd());
    }
}