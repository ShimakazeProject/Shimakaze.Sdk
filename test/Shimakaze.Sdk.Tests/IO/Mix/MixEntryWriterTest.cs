using Shimakaze.Sdk.Data.Mix;
using Shimakaze.Sdk.IO.Mix;

namespace Shimakaze.Sdk.Tests.IO.Mix;

[TestClass]
public class MixEntryWriterTest
{
    private readonly MixEntry csf = new(3179499641, 0, 573269);
    private readonly MixEntry lxd = new(913179935, 573280, 85);
    private const string Assets = "Assets";
    private const string MixFile = "test.mix";
    private const string CsfFile = "ra2md.csf";
    private const string LxdFile = "local mix database.dat";

    private const string OutputPath = "Out";

    [TestInitialize]
    public void Startup()
    {
        Directory.CreateDirectory(OutputPath);
    }

    [TestMethod]
    public async Task Test()
    {
        try
        {
            using var tmp = await MixEntryWriter.CreateAsync(new NonSeekableStream()).ConfigureAwait(false);
        }
        catch (System.NotSupportedException e)
        {
            Assert.AreEqual("The stream cannot support Seek!", e.Message);
        }
        try
        {
            using var tmp = await MixEntryWriter.CreateAsync(null!).ConfigureAwait(false);
        }
        catch (System.ArgumentNullException e)
        {
            Assert.AreEqual("stream", e.ParamName);
        }

        await using Stream fs = File.Create(Path.Combine(OutputPath, MixFile));
        using MixEntryWriter writer = await MixEntryWriter.CreateAsync(fs).ConfigureAwait(false);
        Assert.AreEqual(4 + 2 + 4, fs.Position);
        await writer.WriteAsync(csf);
        Assert.AreEqual(4 + 2 + 4 + 12, fs.Position);
        await writer.WriteAsync(lxd);
        Assert.AreEqual(4 + 2 + 4 + 12 + 12, fs.Position);
        var bodyOffset = fs.Position;

        await using var ra2mdfs = File.OpenRead(Path.Combine(Assets, CsfFile));
        fs.Seek(bodyOffset, SeekOrigin.Begin);
        fs.Seek(csf.Offset, SeekOrigin.Current);
        Assert.AreEqual(bodyOffset + csf.Offset, fs.Position);
        await ra2mdfs.CopyToAsync(fs);
        Assert.AreEqual(bodyOffset + csf.Offset + csf.Size, fs.Position);

        await using var lxdfs = File.OpenRead(Path.Combine(Assets, LxdFile));
        fs.Seek(bodyOffset, SeekOrigin.Begin);
        fs.Seek(lxd.Offset, SeekOrigin.Current);
        Assert.AreEqual(bodyOffset + lxd.Offset, fs.Position);
        await lxdfs.CopyToAsync(fs);
        Assert.AreEqual(bodyOffset + lxd.Offset + lxd.Size, fs.Position);

        await writer.WriteBodySize(new[] { csf, lxd }.Max(i => i.Offset + i.Size)).ConfigureAwait(false);
        await writer.WriteCount(2).ConfigureAwait(false);

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