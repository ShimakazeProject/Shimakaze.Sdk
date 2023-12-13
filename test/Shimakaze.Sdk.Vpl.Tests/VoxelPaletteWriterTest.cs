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
    public async Task StartupAsync()
    {
        Directory.CreateDirectory(OutputPath);
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));

        using VoxelPaletteReader reader = new(stream);

        _vpl = await reader.ReadAsync();
    }

    [TestMethod]
    public async Task WriteTestAsync()
    {
        using (Stream stream = File.Create(Path.Combine(OutputPath, OutputFile)))
        using (VoxelPaletteWriter writer = new(stream))
            await writer.WriteAsync(_vpl);

        var a = BitConverter.ToString(SHA256.HashData(File.ReadAllBytes(Path.Combine(Assets, InputFile))));
        var b = BitConverter.ToString(SHA256.HashData(File.ReadAllBytes(Path.Combine(OutputPath, OutputFile))));

        Assert.AreEqual(a, b, true, CultureInfo.InvariantCulture);
    }
}