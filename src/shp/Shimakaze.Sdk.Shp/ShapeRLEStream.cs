namespace Shimakaze.Sdk.Shp;

/// <summary>
/// RLE 压缩/解压流
/// </summary>
/// <param name="baseStream">基础流</param>
/// <param name="leaveOpen"></param>
public class ShapeRLEStream(Stream baseStream, bool leaveOpen = false) : Stream
{
    private bool _disposed;
    /// <inheritdoc/>
    public override bool CanRead => baseStream.CanRead;

    /// <inheritdoc/>
    public override bool CanSeek => false;

    /// <inheritdoc/>
    public override bool CanWrite => baseStream.CanWrite;

    /// <inheritdoc/>
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc/>
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        FlushRLE();
        baseStream.Flush();
    }

    private byte _readCount;

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        var span = buffer.AsSpan(offset, count);
        for (int i = 0; i < span.Length;)
        {
            if (_readCount is 0)
            {
                var b = baseStream.ReadByte();
                if (b is -1)
                    return i; // End of stream

                if (b is not 0)
                {
                    span[i] = (byte)b;
                    i++;
                    continue;
                }

                var size = baseStream.ReadByte();
                if (size is -1)
                    return i; // End of stream

                _readCount = (byte)size;
            }

            var end = i + _readCount;
            while (i < end)
            {
                span[i] = 0;
                i++;
                _readCount--;
            }
        }

        return count;
    }

    private byte _writeCount;
    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        var span = buffer.AsSpan(offset, count);
        for (int i = 0; i < span.Length; i++)
        {
            ref byte current = ref span[i];
            if (current is 0)
            {
                _writeCount++;
                continue;
            }

            FlushRLE();
            baseStream.WriteByte(current);
        }
    }

    private void FlushRLE()
    {
        if (_writeCount is not 0)
        {
            baseStream.WriteByte(0);
            baseStream.WriteByte(_writeCount);
            _writeCount = 0;
        }
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        _disposed = true;

        if (disposing)
        {
            if (!leaveOpen)
                baseStream.Dispose();
        }

        base.Dispose(disposing);
    }
}
