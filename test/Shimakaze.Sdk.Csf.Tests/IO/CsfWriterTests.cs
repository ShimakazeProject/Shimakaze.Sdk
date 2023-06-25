using System.Security.Cryptography;

using Shimakaze.Sdk.Csf;

namespace Shimakaze.Sdk.IO.Csf;

[TestClass]
public class CsfWriterTests
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";

    private const string OutputPath = "Out";
    private const string OutputFile = "WriteTest.csf";
    private CsfDocument _csf;

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        _csf = reader.Read();
    }


    [TestMethod]
    public void WriteTest()
    {
        using (Stream stream = File.Create(Path.Combine(OutputPath, OutputFile)))
        using (CsfWriter writer = new(stream))
            writer.Write(_csf);

        var a = BitConverter.ToString(MD5.HashData(File.ReadAllBytes(Path.Combine(Assets, InputFile))));
        var b = BitConverter.ToString(MD5.HashData(File.ReadAllBytes(Path.Combine(OutputPath, OutputFile))));

        Assert.AreEqual(a, b, true);
    }

}