using System.IO;

namespace Shimakaze.Sdk.Data.Csf.Serialization.Tests;

[TestClass()]
public class CsfSerializerTests
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";
    private const string OutputPath = "Out";
    private readonly CsfDocument document;


    public CsfSerializerTests()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        this.document = CsfSerializer.Deserialize(stream);

        if (!Directory.Exists(OutputPath))
        {
            Directory.CreateDirectory(OutputPath);
        }
    }


    [TestMethod()]
    public void DeserializeTest()
    {
        using Stream fs = File.OpenRead(path: Path.Combine(Assets, InputFile));
        using BinaryReader br = new(fs);
        CsfDocument csf = CsfSerializer.Deserialize(br);
    }

    [TestMethod()]
    public void DeserializeTest1()
    {
        using Stream fs = File.OpenRead(path: Path.Combine(Assets, InputFile));
        CsfDocument csf = CsfSerializer.Deserialize(fs);
    }

    [TestMethod()]
    public void SerializeTest()
    {
        string path = Path.Combine(OutputPath, "SerializeTest.csf");
        using Stream stream = File.Create(path);
        using BinaryWriter bw = new(stream);
        CsfSerializer.Serialize(bw, this.document);
    }

    [TestMethod()]
    public void SerializeTest1()
    {
        string path = Path.Combine(OutputPath, "SerializeTest1.csf");
        using Stream stream = File.Create(path);
        CsfSerializer.Serialize(stream, this.document);
    }
}
