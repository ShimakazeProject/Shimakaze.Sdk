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
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        _csf = CsfReader.Read(stream);
    }

    [TestMethod]
    public void WriteTest()
    {
        using (Stream stream = File.Create(Path.Combine(OutputPath, OutputFile)))
        {
            CsfWriter.Write(stream, _csf);
        }

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
            fs1.ReadExactly(buffer1);
            fs2.ReadExactly(buffer2);
            Assert.IsTrue(buffer1.SequenceEqual(buffer2),
                $"At Position: 0x{fs1.Position:X8}, BufferSize: {buffer1.Length}, Should be {BitConverter.ToString(buffer1.ToArray())}, but {BitConverter.ToString(buffer2.ToArray())}");
        }
    }
}