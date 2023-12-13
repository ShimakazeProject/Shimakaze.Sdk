using System.Text;

namespace Shimakaze.Sdk.Ini.Ares.Tests;

[TestClass()]
public class AresIniTokenWriterTests
{
    [TestMethod()]
    public void WriteTest()
    {
        using MemoryStream ms = new();

        using StreamWriter sw = new(ms);
        using AresIniTokenWriter writer = new(sw);
        writer.Write(IniTokenTools.BeginBracket);
        writer.Write(IniTokenTools.Value("Section"));
        writer.WriteLine(IniTokenTools.EndBracket);
        writer.Flush();

        ms.Seek(0, SeekOrigin.Begin);
        Assert.AreEqual("[Section]" + sw.NewLine, Encoding.UTF8.GetString(ms.ToArray()));
    }
}