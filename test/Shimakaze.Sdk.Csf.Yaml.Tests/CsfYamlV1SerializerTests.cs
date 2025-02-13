using Shimakaze.Sdk.Csf.IO;

namespace Shimakaze.Sdk.Csf.Yaml.Tests;

[TestClass]
public class CsfYamlV1WriterTests
{
    private const string Assets = "Assets";
    private const string InputCsfFile = "ra2md.csf";
    private const string InputYmlFile = "ra2md.v1.csf.yaml";
    private const string OutputDeserializeCsfFile = "DeserializeTest.v1.csf";
    private const string OutputDeserializeYamlFile = "DeserializeTest.v1.csf.yml";
    private const string OutputPath = "Out";
    private const string OutputSerializeFile = "SerializeTest.v1.csf.yml";

    [TestMethod]
    public void DeserializeTest()
    {
        using StreamReader stream = File.OpenText(Path.Combine(Assets, InputYmlFile));
        using Stream csfout = File.Create(Path.Combine(OutputPath, OutputDeserializeCsfFile));
        using CsfWriter writer = new(csfout);
        using StreamWriter ymlout = File.CreateText(Path.Combine(OutputPath, OutputDeserializeYamlFile));
        CsfData doc = CsfYamlV1Reader.Read(stream);
        writer.WriteAllData(doc);
        CsfYamlV1Writer.Write(ymlout, doc);
    }

    [TestMethod]
    public void SerializeTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputCsfFile));
        using CsfReader reader = new(stream);
        using StreamWriter output = File.CreateText(Path.Combine(OutputPath, OutputSerializeFile));
        CsfData document = reader.ReadAllData();
        CsfYamlV1Writer.Write(output, document);
    }

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }
}
