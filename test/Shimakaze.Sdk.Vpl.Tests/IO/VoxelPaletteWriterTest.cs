using System.Security.Cryptography;

using Shimakaze.Sdk.Vpl;

namespace Shimakaze.Sdk.IO.Vpl.Tests;


[TestClass]
public sealed class VoxelPaletteWriterTest
{
    private const string OutputPath = "Out";
    private const string OutputFile = "voxels.vpl";

    private const string Assets = "Assets";
    private const string InputFile = "voxels.vpl";

    private VoxelPalette _vpl;

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));

        using VoxelPaletteReader reader = new(stream);

        _vpl = reader.Read();
    }


    [TestMethod]
    public void WriteTest()
    {
        using (Stream stream = File.Create(Path.Combine(OutputPath, OutputFile)))
        using (VoxelPaletteWriter writer = new(stream))
            writer.Write(_vpl);

        var a = BitConverter.ToString(MD5.HashData(File.ReadAllBytes(Path.Combine(Assets, InputFile))));
        var b = BitConverter.ToString(MD5.HashData(File.ReadAllBytes(Path.Combine(OutputPath, OutputFile))));

        Assert.AreEqual(a, b, true);
    }
}