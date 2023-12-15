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
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));

        using VoxelReader reader = new(stream);

        _vxl = reader.Read();
    }

    [TestMethod]
    public unsafe void SizeOfTest()
    {
        Assert.AreEqual(6, sizeof(Bounds));
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
        using (VoxelWriter writer = new(stream))
            writer.Write(_vxl);

        Compare(Path.Combine(Assets, InputFile), Path.Combine(OutputPath, OutputFile));
    }

    private void Compare(string path1, string path2)
    {
        Span<byte> buffer1 = stackalloc byte[8];
        Span<byte> buffer2 = stackalloc byte[8];

        using Stream fs1 = File.OpenRead(path1);
        using Stream fs2 = File.OpenRead(path2);
        Assert.AreEqual(fs1.Length, fs2.Length);

        while (fs1.Position < fs1.Length)
        {
            fs1.Read(buffer1);
            fs2.Read(buffer2);
            Assert.IsTrue(buffer1.SequenceEqual(buffer2), $"At Position: {fs1.Position}, BufferSize£º {buffer1.Length}");
        }
    }
}