using System.Globalization;
using System.Security.Cryptography;

namespace Shimakaze.Sdk.Vpl.Tests;

[TestClass]
public sealed class VoxelPaletteWriterTest
{
    private const string Assets = "Assets";
    private const string InputFile = "voxels.vpl";
    private const string OutputFile = "voxels.vpl";
    private const string OutputPath = "Out";
    private VoxelPalette _vpl = default!;

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
            Assert.IsTrue(buffer1.SequenceEqual(buffer2),
                $"At Position: {fs1.Position}, BufferSize£º {buffer1.Length}, Should be {BitConverter.ToString(buffer1.ToArray())}, but {BitConverter.ToString(buffer2.ToArray())}");
        }
    }
}