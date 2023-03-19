using Microsoft.VisualStudio.TestTools.UnitTesting;

using Shimakaze.Sdk.Data.Csf.Serialization;
using Shimakaze.Sdk.Data.Csf;
using Shimakaze.Sdk.Text.Csf.Xml.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shimakaze.Sdk.Tests.Text.Csf.Xml.Serialization;

[TestClass]
public class CsfXmlV1SerializerTests
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";
    private const string InputFile2 = "ra2md.v1.csf.xml";
    private const string OutputPath = "Out";
    private const string OutputDeserializeCsfFile1 = "DeserializeTest1.v1.csf";
    private const string OutputDeserializeXmlFile1 = "DeserializeTest1.v1.csf.xml";
    private const string OutputDeserializeCsfFile2 = "DeserializeTest2.v1.csf";
    private const string OutputDeserializeXmlFile2 = "DeserializeTest2.v1.csf.xml";
    private const string OutputSerializeFile1 = "SerializeTest1.v1.csf.xml";
    private const string OutputSerializeFile2 = "SerializeTest2.v1.csf.xml";

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public void DeserializeTest1()
    {
        using StreamReader sr = File.OpenText(Path.Combine(Assets, InputFile2));
        CsfDocument? doc = CsfXmlV1Serializer.Deserialize(sr);
        Assert.IsNotNull(doc);
        using Stream output = File.Create(Path.Combine(OutputPath, OutputDeserializeCsfFile1));
        using Stream output1 = File.Create(Path.Combine(OutputPath, OutputDeserializeXmlFile1));
        CsfSerializer.Serialize(output, doc);
        CsfXmlV1Serializer.Serialize(output1, doc);
    }

    [TestMethod]
    public void DeserializeTest2()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile2));
        CsfDocument? doc = CsfXmlV1Serializer.Deserialize(stream);
        Assert.IsNotNull(doc);
        using Stream output = File.Create(Path.Combine(OutputPath, OutputDeserializeCsfFile2));
        using Stream output1 = File.Create(Path.Combine(OutputPath, OutputDeserializeXmlFile2));
        CsfSerializer.Serialize(output, doc);
        CsfXmlV1Serializer.Serialize(output1, doc);
    }

    [TestMethod]
    public void SerializeTest1()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        CsfDocument document = CsfSerializer.Deserialize(stream);

        string path = Path.Combine(OutputPath, OutputSerializeFile1);
        using StreamWriter sw = File.CreateText(path);
        CsfXmlV1Serializer.Serialize(sw, document);
    }

    [TestMethod]
    public void SerializeTest2()
    {
        using Stream stream1 = File.OpenRead(Path.Combine(Assets, InputFile));
        CsfDocument document = CsfSerializer.Deserialize(stream1);

        string path = Path.Combine(OutputPath, OutputSerializeFile2);
        using Stream stream = File.Create(path);
        CsfXmlV1Serializer.Serialize(stream, document);
    }
}