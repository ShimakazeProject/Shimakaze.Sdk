namespace Shimakaze.Sdk.Vxl.Tests;

[TestClass]
public sealed class VoxelWriterTest
{
    private const string Assets = "Assets";
    private const string InputFile = "jeep.vxl";
    private const string OutputFile = "jeep.vxl";
    private const string OutputPath = "Out";
    private VoxelFile _vxl = default!;

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
        using FileStream stream = File.OpenRead(Path.Combine(Assets, InputFile));

        _vxl = VoxelReader.Read(stream);
    }

    [TestMethod]
    public unsafe void SizeOfTest()
    {
        Assert.AreEqual(24, sizeof(Bounds));
        Assert.AreEqual(28, sizeof(SectionHeader));
        Assert.AreEqual(16, sizeof(SectionName));
        Assert.AreEqual(92, sizeof(SectionTailer));
        Assert.AreEqual(48, sizeof(Transform));
        Assert.AreEqual(2, sizeof(Voxel));
        Assert.AreEqual(3, sizeof(VoxelSize));
        Assert.AreEqual(34, sizeof(VoxelHeader));
    }

    [TestMethod]
    public void WriteTest()
    {
        using (Stream stream = File.Create(Path.Combine(OutputPath, OutputFile)))
        {
            VoxelWriter.Write(_vxl, stream);
        }

        Compare(Path.Combine(Assets, InputFile), Path.Combine(OutputPath, OutputFile));
    }

    private void Compare(string path1, string path2)
    {
        Span<byte> buffer1 = stackalloc byte[8];
        Span<byte> buffer2 = stackalloc byte[8];

        using Stream fs1 = File.OpenRead(path1);
        using Stream fs2 = File.OpenRead(path2);
        Assert.AreEqual(fs1.Length, fs2.Length);

        int size = (int)fs1.Length;
        while (size > 0)
        {
            fs1.ReadExactly(buffer1[..Math.Min(size, 8)]);
            fs2.ReadExactly(buffer2[..Math.Min(size, 8)]);
            size -= 8;
            Assert.IsTrue(buffer1.SequenceEqual(buffer2),
                $"At Position: 0x{fs1.Position:X8}, BufferSize: {buffer1.Length}, Should be {BitConverter.ToString(buffer1.ToArray())}, but {BitConverter.ToString(buffer2.ToArray())}");
        }
    }
}
