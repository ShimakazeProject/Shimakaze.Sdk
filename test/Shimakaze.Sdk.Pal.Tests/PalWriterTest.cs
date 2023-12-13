using System.Globalization;
using System.Security.Cryptography;

using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Pal.Tests;

[TestClass]
public sealed class PalWriterTest
{
    private const string Assets = "Assets";
    private const string InputFile = "unittem.pal";
    private const string OutputFile = "unittem.pal";
    private const string OutputPath = "Out";
    private Palette _pal = default!;

    [TestInitialize]
    public async Task StartupAsync()
    {
        Directory.CreateDirectory(OutputPath);
        using var stream = File.OpenRead(Path.Combine(Assets, InputFile));

        using PaletteReader reader = new(stream);

        _pal = await reader.ReadAsync();
    }

    [TestMethod]
    public async Task WriteTestAsync()
    {
        using (Stream stream = File.Create(Path.Combine(OutputPath, OutputFile)))
        using (PaletteWriter writer = new(stream))
            await writer.WriteAsync(_pal);

        var a = BitConverter.ToString(SHA256.HashData(File.ReadAllBytes(Path.Combine(Assets, InputFile))));
        var b = BitConverter.ToString(SHA256.HashData(File.ReadAllBytes(Path.Combine(OutputPath, OutputFile))));

        Assert.AreEqual(a, b, true, CultureInfo.InvariantCulture);
    }
}