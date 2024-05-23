namespace Shimakaze.Sdk.Common.Tests;

#pragma warning disable IDISP002 // Dispose member
[TestClass]
public class StreamExtensions : IDisposable
{
    private Stream _stream = default!;
    private bool _disposedValue;

    [TestInitialize]
    public void Initialize()
    {
        _stream?.Dispose();
        _stream = new MemoryStream();
        _stream.WriteByte(0);
        _stream.WriteByte(1);
        _stream.WriteByte(0);
        _stream.WriteByte(0);
        _stream.Seek(0, SeekOrigin.Begin);
    }

    [TestMethod]
    public void ReadTest1()
    {
        _stream.Read(out int o);
        Assert.AreEqual(256, o);
    }
    [TestMethod]
    public void ReadTest2()
    {
        short[] o = new short[2];
        _stream.Read(o);
        Assert.AreEqual(256, o[0]);
        Assert.AreEqual(0, o[1]);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _stream?.Dispose();
            }

            _disposedValue = true;
        }
    }

    // ~StreamExtensions()
    // {
    //     // ��Ҫ���Ĵ˴��롣�뽫���������롰Dispose(bool disposing)��������
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // ��Ҫ���Ĵ˴��롣�뽫���������롰Dispose(bool disposing)��������
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}