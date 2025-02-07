using Shimakaze.Sdk.Csf.IO;

namespace Shimakaze.Sdk.Csf.Xml.Tests;

[TestClass]
public class CsfXmlV1WriterTests
{
    private const string Assets = "Assets";
    private const string InputCsfFile = "ra2md.csf";
    private const string InputXmlFile = "ra2md.v1.csf.xml";
    private const string OutputDeserializeCsfFile = "DeserializeTest.v1.csf";
    private const string OutputDeserializeXmlFile = "DeserializeTest.v1.csf.xml";
    private const string OutputPath = "Out";
    private const string OutputSerializeFile = "SerializeTest.v1.csf.xml";

    [TestMethod]
    public void DeserializeTest()
    {
        using StreamReader stream = File.OpenText(Path.Combine(Assets, InputXmlFile));
        using StreamWriter xmlout = File.CreateText(Path.Combine(OutputPath, OutputDeserializeXmlFile));
        using Stream csfout = File.Create(Path.Combine(OutputPath, OutputDeserializeCsfFile));
        using CsfWriter writer = new(csfout);
        CsfData doc = CsfXmlV1Reader.Read(stream);
        Assert.IsNotNull(doc);
        writer.WriteAllData(doc);
        CsfXmlV1Writer.Write(xmlout, doc);
    }

    [TestMethod]
    public void SerializeTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputCsfFile));
        using CsfReader reader = new(stream);
        using StreamWriter output = File.CreateText(Path.Combine(OutputPath, OutputSerializeFile));
        CsfData document = reader.ReadAllData();
        CsfXmlV1Writer.Write(output, document);
    }

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }
}
