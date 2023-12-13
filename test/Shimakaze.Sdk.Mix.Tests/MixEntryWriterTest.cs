
namespace Shimakaze.Sdk.Mix.Tests;

[TestClass]
public class MixEntryWriterTest
{
    private const string Assets = "Assets";
    private const string CsfFile = "ra2md.csf";
    private const string LxdFile = "local mix database.dat";
    private const string MixFile = "test.mix";
    private const string OutputPath = "Out";
    private readonly MixEntry _csf = new(3179499641, 0, 573269);
    private readonly MixEntry _lxd = new(913179935, 573280, 85);

    [TestMethod]
    public async Task InitTestAsync()
    {
        using MemoryStream ms = new();
        using MixEntryWriter writer = new(ms);
        await writer.WriteAsync(_csf);
    }

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public async Task Test()
    {
        Assert.ThrowsException<NotSupportedException>(() =>
        {
            using NonSeekableStream stream = new();
            using MixEntryWriter tmp = new(stream);
        });

        await using Stream fs = File.Create(Path.Combine(OutputPath, MixFile));
        using MixEntryWriter writer = new(fs);
        writer.Init();
        Assert.AreEqual(4 + 2 + 4, fs.Position);
        await writer.WriteAsync(_csf);
        Assert.AreEqual(4 + 2 + 4 + 12, fs.Position);
        await writer.WriteAsync(_lxd);
        Assert.AreEqual(4 + 2 + 4 + 12 + 12, fs.Position);
        var bodyOffset = fs.Position;

        await using var ra2mdfs = File.OpenRead(Path.Combine(Assets, CsfFile));
        fs.Seek(bodyOffset, SeekOrigin.Begin);
        fs.Seek(_csf.Offset, SeekOrigin.Current);
        Assert.AreEqual(bodyOffset + _csf.Offset, fs.Position);
        await ra2mdfs.CopyToAsync(fs);
        Assert.AreEqual(bodyOffset + _csf.Offset + _csf.Size, fs.Position);

        await using var lxdfs = File.OpenRead(Path.Combine(Assets, LxdFile));
        fs.Seek(bodyOffset, SeekOrigin.Begin);
        fs.Seek(_lxd.Offset, SeekOrigin.Current);
        Assert.AreEqual(bodyOffset + _lxd.Offset, fs.Position);
        await lxdfs.CopyToAsync(fs);
        Assert.AreEqual(bodyOffset + _lxd.Offset + _lxd.Size, fs.Position);

        writer.WriteMetadata();

        await using var mixfs = File.OpenRead(Path.Combine(Assets, MixFile));
        fs.Seek(0, SeekOrigin.Begin);
        while (mixfs.Position < mixfs.Length)
        {
            var _1 = fs.ReadByte();
            var _2 = mixfs.ReadByte();
            Assert.AreEqual(_2, _1, $"At Position {fs.Position}");
        }
    }
}

file sealed class NonSeekableStream : Stream
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