using Shimakaze.Sdk.Text.Ini;
using Shimakaze.Sdk.Text.Ini.Serialization;

namespace Shimakaze.Sdk.Tests.Text.Ini;

[TestClass]
public class IniWriterTests
{
    private const string OutputPath = "Out";
    private const string OutputFile1 = "IniWriterTest1.ini";
    private const string OutputFile2 = "IniWriterTest2.ini";
    private const string OutputFile3 = "IniWriterTest3.ini";

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public void IniWriterTest1()
    {
        using StreamWriter sw = File.CreateText(Path.Combine(OutputPath, OutputFile1));
        IniWriter writer = new(sw);
    }

    [TestMethod]
    public void IniWriterTest2()
    {
        using StreamWriter sw = File.CreateText(Path.Combine(OutputPath, OutputFile2));
        IniWriter writer = new(sw, IniSerializerOptions.Default);
    }

    [TestMethod]
    public void WriteTest()
    {
        using StreamWriter sw = File.CreateText(Path.Combine(OutputPath, OutputFile3));
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
