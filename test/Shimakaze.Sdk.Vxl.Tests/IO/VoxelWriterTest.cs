using System.Security.Cryptography;

using Shimakaze.Sdk.Vxl;

namespace Shimakaze.Sdk.IO.Vxl.Tests;


[TestClass]
public sealed class VoxelWriterTest
{
    private const string OutputPath = "Out";
    private const string OutputFile = "jeep.vxl";

    private const string Assets = "Assets";
    private const string InputFile = "jeep.vxl";

    private VXLFile _vxl;

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));

        using VoxelReader reader = new(stream);

        _vxl = reader.Read();
    }


    [TestMethod]
    public void WriteTest()
    {
        using (Stream stream = File.Create(Path.Combine(OutputPath, OutputFile)))
        using (VoxelWriter writer = new(stream))
            writer.Write(_vxl);

        var a = BitConverter.ToString(MD5.HashData(File.ReadAllBytes(Path.Combine(Assets, InputFile))));
        var b = BitConverter.ToString(MD5.HashData(File.ReadAllBytes(Path.Combine(OutputPath, OutputFile))));

        Assert.AreEqual(a, b, true);
    }
}