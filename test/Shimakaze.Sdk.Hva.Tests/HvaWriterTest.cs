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

        Assert.IsTrue(Compare(Path.Combine(Assets, InputFile), Path.Combine(OutputPath, OutputFile)));
    }

    private bool Compare(string path1, string path2)
    {
        Span<byte> buffer1 = stackalloc byte[8];
        Span<byte> buffer2 = stackalloc byte[8];

        using Stream fs1 = File.OpenRead(Path.Combine(Assets, InputFile));
        using Stream fs2 = File.OpenRead(Path.Combine(OutputPath, OutputFile));
        if (fs1.Length != fs2.Length)
            return false;

        while (fs1.Position < fs1.Length)
        {
            fs1.Read(buffer1);
            fs2.Read(buffer2);
            if (!buffer1.SequenceEqual(buffer2))
                return false;
        }
        return true;
    }
}