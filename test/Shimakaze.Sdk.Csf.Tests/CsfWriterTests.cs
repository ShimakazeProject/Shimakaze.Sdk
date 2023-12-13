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

        var a = BitConverter.ToString(SHA256.HashData(File.ReadAllBytes(Path.Combine(Assets, InputFile))));
        var b = BitConverter.ToString(SHA256.HashData(File.ReadAllBytes(Path.Combine(OutputPath, OutputFile))));

        Assert.AreEqual(a, b, true, CultureInfo.InvariantCulture);
    }
}