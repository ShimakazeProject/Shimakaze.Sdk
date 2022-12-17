namespace Shimakaze.Sdk.Text.Ini.Tests;

[TestClass]
public class IniWriterTests
{
    private const string OutputPath = "Out";

    public IniWriterTests()
    {
        if (!Directory.Exists(OutputPath))
        {
            Directory.CreateDirectory(OutputPath);
        }
    }

    [TestMethod]
    public void IniWriterTest()
    {
        using StreamWriter sw = File.CreateText(Path.Combine(OutputPath, "IniWriterTest.ini"));
        IniWriter writer = new(sw);
    }

    [TestMethod]
    public void IniWriterTest1()
    {
        using StreamWriter sw = File.CreateText(Path.Combine(OutputPath, "IniWriterTest1.ini"));
        IniWriter writer = new(sw, Serialization.IniSerializerOptions.Default);
    }

    [TestMethod]
    public void WriteTest()
    {
        using StreamWriter sw = File.CreateText(Path.Combine(OutputPath, "WriteTest.ini"));
        IniWriter writer = new(sw);
        writer.Write(IniToken.Comment, "File Comment");
        writer.Write(IniToken.EmptyLine, string.Empty);
        writer.Write(IniToken.SectionHeader, "Section");
        writer.Write(IniToken.Comment, "Section Comment");
        writer.Write(IniToken.EmptyLine, string.Empty);
        writer.Write(IniToken.Key, "Key");
        writer.Write(IniToken.Value, "Value");
        writer.Write(IniToken.Comment, "Inline Comment");
        writer.Write(IniToken.EmptyLine, string.Empty);
    }
}
