namespace Shimakaze.Sdk.Vxl.Tests;

[TestClass]
public sealed class VoxelReaderTest
{
    private const string Assets = "Assets";
    private const string InputFile = "jeep.vxl";

    [TestMethod]
    public void ReadTest()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));

        var res = VoxelReader.Read(stream);
    }
}