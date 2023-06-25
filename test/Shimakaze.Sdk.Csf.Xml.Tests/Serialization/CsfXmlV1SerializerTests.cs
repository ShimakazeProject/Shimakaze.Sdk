using System.Text.Unicode;

using Shimakaze.Sdk.Csf;
using Shimakaze.Sdk.Csf.Xml.Serialization;
using Shimakaze.Sdk.IO.Csf;

namespace Shimakaze.Sdk.Tests.Text.Csf.Xml.Serialization;

[TestClass]
public class CsfXmlV1SerializerTests
{
    private const string Assets = "Assets";
    private const string InputCsfFile = "ra2md.csf";
    private const string InputXmlFile = "ra2md.v1.csf.xml";
    private const string OutputPath = "Out";
    private const string OutputDeserializeCsfFile = "DeserializeTest.v1.csf";
    private const string OutputDeserializeXmlFile = "DeserializeTest.v1.csf.xml";
    private const string OutputSerializeFile = "SerializeTest.v1.csf.xml";

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public void DeserializeTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputXmlFile));
        using Stream xmlout = File.Create(Path.Combine(OutputPath, OutputDeserializeXmlFile));
        using Stream csfout = File.Create(Path.Combine(OutputPath, OutputDeserializeCsfFile));
        using CsfXmlV1Deserializer deserializer = new(stream);
        using CsfXmlV1Serializer xmlSerializer = new(xmlout);
        using CsfWriter writer = new(csfout);
        CsfDocument doc = deserializer.Deserialize();
        Assert.IsNotNull(doc);
        writer.Write(doc);
        xmlSerializer.Serialize(doc);
    }

    [TestMethod]
    public void SerializeTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputCsfFile));
        using Stream output = File.Create(Path.Combine(OutputPath, OutputSerializeFile));
        using CsfReader reader = new(stream);
        using CsfXmlV1Serializer serializer = new(output);
        CsfDocument document = reader.Read();
        serializer.Serialize(document);
    }

    //[TestMethod]
    //public void OtherTest()
    //{
    //    using Stream stream = new MemoryStream("""
    //        <Resources protocol="1" version="3" language="0">
    //            <Label name="Extra:Test" extra="Extra">Value</Label>
    //        </Resources>
    //        """u8.ToArray());
    //    using Stream output = File.Create(Path.Combine(OutputPath, OutputSerializeFile));
    //    using CsfDeserializer deserializer = new(stream);
    //    using CsfXmlV1Serializer serializer = new(output);
    //    CsfDocument document = deserializer.Deserialize();
    //    serializer.Serialize(document);
    //}
}