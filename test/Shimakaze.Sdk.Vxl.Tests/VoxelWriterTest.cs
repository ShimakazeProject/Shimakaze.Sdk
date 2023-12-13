using System.Globalization;
using System.Security.Cryptography;

namespace Shimakaze.Sdk.Vxl.Tests;

[TestClass]
public sealed class VoxelWriterTest
{
    private const string Assets = "Assets";
    private const string InputFile = "jeep.vxl";
    private const string OutputFile = "jeep.vxl";
    private const string OutputPath = "Out";
    private VXLFile _vxl = default!;

    [TestInitialize]
    public async Task StartupAsync()
    {
        Directory.CreateDirectory(OutputPath);
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));

        using VoxelReader reader = new(stream);

        _vxl = await reader.ReadAsync();
    }

    [TestMethod]
    public async Task WriteTestAsync()
    {
        using (Stream stream = File.Create(Path.Combine(OutputPath, OutputFile)))
        using (VoxelWriter writer = new(stream))
            await writer.WriteAsync(_vxl);

        var a = BitConverter.ToString(SHA256.HashData(File.ReadAllBytes(Path.Combine(Assets, InputFile))));
        var b = BitConverter.ToString(SHA256.HashData(File.ReadAllBytes(Path.Combine(OutputPath, OutputFile))));

        Assert.AreEqual(a, b, true, CultureInfo.InvariantCulture);
    }
}