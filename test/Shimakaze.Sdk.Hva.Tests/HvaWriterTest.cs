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