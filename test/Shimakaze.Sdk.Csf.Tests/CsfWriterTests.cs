using System.Globalization;
using System.Security.Cryptography;

using Shimakaze.Sdk.Csf;

namespace Shimakaze.Sdk.Csf.Tests;

[TestClass]
public class CsfWriterTests
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";

    private const string OutputFile = "WriteTest.csf";
    private const string OutputPath = "Out";
    private CsfDocument _csf = default!;

    [TestInitialize]
    public async Task StartupAsync()
    {
        Directory.CreateDirectory(OutputPath);
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        _csf = await reader.ReadAsync();
    }

    [TestMethod]
    public async Task WriteTestAsync()
    {
        using (Stream stream = File.Create(Path.Combine(OutputPath, OutputFile)))
        using (CsfWriter writer = new(stream))
            await writer.WriteAsync(_csf);

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