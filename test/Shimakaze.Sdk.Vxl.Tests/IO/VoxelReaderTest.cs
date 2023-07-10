namespace Shimakaze.Sdk.IO.Vxl.Tests;

[TestClass]
public sealed class VoxelReaderTest
{
    private const string Assets = "Assets";
    private const string InputFile = "jeep.vxl";

    [TestMethod]
    public async Task ReadTestAsync()
    {
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));

        using VoxelReader reader = new(stream);

        var res = await reader.ReadAsync();
    }
}