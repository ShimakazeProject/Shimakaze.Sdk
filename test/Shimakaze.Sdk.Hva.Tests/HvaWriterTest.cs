using System.Globalization;
using System.Security.Cryptography;

namespace Shimakaze.Sdk.Hva.Tests;

[TestClass]
public sealed class HvaWriterTest
{
    private const string Assets = "Assets";
    private const string InputFile = "jeep.hva";
    private const string OutputFile = "jeep.hva";
    private const string OutputPath = "Out";
    private HvaFile _hva = default!;

    [TestInitialize]
    public async Task StartupAsync()
    {
        Directory.CreateDirectory(OutputPath);
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));

        using HvaReader reader = new(stream);

        _hva = await reader.ReadAsync();
    }

    [TestMethod]
    public async Task WriteTestAsync()
    {
        using (Stream stream = File.Create(Path.Combine(OutputPath, OutputFile)))
        using (HvaWriter writer = new(stream))
            await writer.WriteAsync(_hva);

        var a = BitConverter.ToString(SHA256.HashData(File.ReadAllBytes(Path.Combine(Assets, InputFile))));
        var b = BitConverter.ToString(SHA256.HashData(File.ReadAllBytes(Path.Combine(OutputPath, OutputFile))));

        Assert.AreEqual(a, b, true, CultureInfo.InvariantCulture);
    }
}