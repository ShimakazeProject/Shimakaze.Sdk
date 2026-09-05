using System.Diagnostics;

namespace Shimakaze.Sdk.Pcx;

/// <summary>
/// RLE 压缩/解压流
/// </summary>
/// <param name="baseStream">基础流</param>
/// <param name="leaveOpen"></param>
public class PcxRLEStream(Stream baseStream, bool leaveOpen = false) : Stream
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

    private byte _readByte;
    private byte _readCount;

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        var span = buffer.AsSpan(offset, count);
        for (int i = 0; i < span.Length;)
        {
            if (_readCount is 0)
            {
                int flag = baseStream.ReadByte();

                if (flag is -1)
                    return i; // End of stream

                if ((flag & 0b11000000) is not 0b11000000)
                {
                    span[i] = (byte)flag;
                    i++;
                    continue;
                }

                _readCount = (byte)(flag & 0b00111111);
                if (_readCount is 0)
                    throw new InvalidDataException("RLE size cannot be 0.");

                int b = baseStream.ReadByte();
                if (b is -1)
                    return i; // End of stream

                _readByte = (byte)b;
            }

            int end = i + _readCount;
            while (i < end)
            {
                span[i] = _readByte;
                i++;
                _readCount--;
            }
        }

        return count;
    }

    private byte _writeByte;
    private byte _writeCount;
    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        var span = buffer.AsSpan(offset, count);
        for (int i = 0; i < span.Length; i++)
        {
            if (_writeCount == 0)
            {
                _writeByte = span[i];
                _writeCount++;
            }
            else if (_writeByte != span[i] || _writeCount == 0b00111111)
            {
                FlushRLE();
                _writeCount = 1;
                _writeByte = span[i];
            }
            _writeCount++;
        }
    }

    private void FlushRLE()
    {
        Debug.Assert(_writeCount <= 0b00111111);
        if (_writeCount is not 1)
            baseStream.WriteByte((byte)(0b11000000 + _writeCount));
        else if ((_writeByte & 0b11000000) is 0b11000000)
            baseStream.WriteByte(0b11000000 + 1);

        baseStream.WriteByte(_writeByte);
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
