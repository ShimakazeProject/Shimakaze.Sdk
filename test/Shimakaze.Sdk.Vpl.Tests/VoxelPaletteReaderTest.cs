namespace Shimakaze.Sdk.Vpl.Tests;

[TestClass]
public sealed class VoxelPaletteReaderTest
{
    private const string Assets = "Assets";
    private const string InputFile = "voxels.vpl";

    [TestMethod]
    public void ReadTest()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));

        var res = VoxelPaletteReader.Read(stream);

        Assert.AreEqual(16u, res.Header.RemapPlayerColorStart);
        Assert.AreEqual(31u, res.Header.RemapPlayerColorEnd);
        Assert.AreEqual(32u, res.Header.SectionCount);
        Assert.AreEqual(0xCD_CD_CD_CDu, res.Header.Unknown);
        Assert.AreEqual(256, res.Palette.Colors.Length);
    }
}