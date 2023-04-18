namespace Shimakaze.Sdk.IO.Csf;

[TestClass]
public class CsfReaderTests
{
    private const string Assets = "Assets";
    private const string InputFile = "ra2md.csf";

    [TestMethod]
    public void CsfReaderTest()
    {
        Assert.ThrowsException<NotSupportedException>(() =>
        {
            using Stream ms = new MemoryStream();
            using CsfReader reader = new(ms, false, new byte[1]);
        });
    }
    [TestMethod]
    public void ReadTest()
    {
        using Stream stream = File.OpenRead(Path.Combine(Assets, InputFile));
        using CsfReader reader = new(stream);
        Assert.IsNotNull(reader.Read());
    }
}

[TestClass]
public class CsfWriterTests
{
    private const string OutputPath = "Out";
    private const string OutputFile = "WriteTest.csf";

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public void CsfWriterTest()
    {
        Assert.ThrowsException<NotSupportedException>(() =>
        {
            using Stream ms = new NoSeekStream();
            using CsfWriter writer = new(ms);
        });
    }

    [TestMethod]
    public void WriteTest()
    {
        using Stream stream = File.Create(Path.Combine(OutputPath, OutputFile));
        using CsfWriter writer = new(stream);
        writer.Write(new("Test'"));
        writer.WriteMetadata();
    }

    private sealed class NoSeekStream : Stream
    {
        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => false;

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}