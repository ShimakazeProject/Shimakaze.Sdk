﻿using Shimakaze.Sdk.Data.Csf;
using Shimakaze.Sdk.Data.Csf.Serialization;

namespace Shimakaze.Sdk.Tests.Data.Csf.Serialization;

[TestClass]
public class CsfSerializerTests
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";
    private const string OutputPath = "Out";
    private const string OutputFile1 = "SerializeTest1.csf";
    private const string OutputFile2 = "SerializeTest2.csf";
    private CsfDocument? document;

    [TestInitialize]
    public void Startup()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        document = CsfSerializer.Deserialize(stream);

        if (!Directory.Exists(OutputPath))
        {
            Directory.CreateDirectory(OutputPath);
        }
    }


    [TestMethod]
    public void DeserializeTest1()
    {
        using Stream fs = File.OpenRead(path: Path.Combine(Assets, InputFile));
        using BinaryReader br = new(fs);
        CsfDocument csf = CsfSerializer.Deserialize(br);
    }

    [TestMethod]
    public void DeserializeTest2()
    {
        using Stream fs = File.OpenRead(path: Path.Combine(Assets, InputFile));
        CsfDocument csf = CsfSerializer.Deserialize(fs);
    }

    [TestMethod]
    public void SerializeTest1()
    {
        string path = Path.Combine(OutputPath, OutputFile1);
        using Stream stream = File.Create(path);
        using BinaryWriter bw = new(stream);
        CsfSerializer.Serialize(bw, document!);
    }

    [TestMethod]
    public void SerializeTest2()
    {
        string path = Path.Combine(OutputPath, OutputFile2);
        using Stream stream = File.Create(path);
        CsfSerializer.Serialize(stream, document!);
    }
}
