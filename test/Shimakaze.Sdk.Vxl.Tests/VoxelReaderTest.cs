namespace Shimakaze.Sdk.Vxl.Tests;

[TestClass]
public sealed class VoxelReaderTest
{
    private const string Assets = "Assets";
    private const string InputFile = "jeep.vxl";

    [TestMethod]
    public void ReadTest()
    {
        using FileStream stream = File.OpenRead(Path.Combine(Assets, InputFile));

        VoxelFile res = VoxelReader.Read(stream);
    }
}
