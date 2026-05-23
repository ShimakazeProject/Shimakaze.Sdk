namespace Shimakaze.Sdk.Mix.Blowfish;

internal sealed class BlowfishStream(Stream stream, ReadOnlySpan<byte> key) : Stream
{
    private readonly Codec _codec = new(key);

    public override bool CanRead => stream.CanRead;
    public override bool CanSeek => throw new NotSupportedException();
    public override bool CanWrite => stream.CanWrite;
    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    private void Encrypt(Span<byte> data)
    {
        if (data is not { Length: 8 })
            throw new InvalidDataException();

        _codec.Encrypt(data);
    }

    private void Decrypt(Span<byte> data)
    {
        if (data is not { Length: 8 })
            throw new InvalidDataException();

        _codec.Decrypt(data);

    }

    private readonly Queue<byte> _readBuffer = new(7);
    public override int Read(byte[] buffer, int offset, int count)
    {
        if (count == 0)
            return 0;

        Span<byte> span = buffer.AsSpan(offset, count);
        while (!span.IsEmpty && _readBuffer.Count is not 0)
        {
            span[0] = _readBuffer.Dequeue();
            span = span[1..];
        }

        Span<byte> temp = stackalloc byte[8];
        while (!span.IsEmpty)
        {
            int size = Math.Min(span.Length, temp.Length);
            stream.ReadExactly(temp);
            Decrypt(temp);
            temp[..size].CopyTo(span);
            span = span[size..];
            if (size is not 8)
            {
                temp = temp[size..];
                while (!temp.IsEmpty)
                {
                    _readBuffer.Enqueue(temp[0]);
                    temp = temp[1..];
                }
            }
        }

        return count;
    }

    private readonly Queue<byte> _writeBuffer = [];
    public override void Write(byte[] buffer, int offset, int count)
    {
        Span<byte> span;
        if (_writeBuffer.Count is not 0)
        {
            span = GC.AllocateUninitializedArray<byte>(count + _writeBuffer.Count);
            var tmp = span;
            while (_writeBuffer.Count is not 0)
            {
                tmp[0] = _writeBuffer.Dequeue();
                tmp = tmp[1..];
            }
        }
        else
        {
            span = buffer.AsSpan(offset, count);
        }

        while (!span.IsEmpty)
        {
            if (span.Length is < 8)
            {
                while (!span.IsEmpty)
                {
                    _writeBuffer.Enqueue(span[0]);
                    span = span[1..];
                }

                break;
            }

            var tmp = span[..8];
            Encrypt(tmp);
            stream.Write(tmp);

            span = span[8..];
        }
    }

    public override void Flush()
    {
        if (_writeBuffer.Count is not 0)
        {
            Span<byte> span = stackalloc byte[8];
            var tmp = span;
            while (_writeBuffer.Count is not 0)
            {
                tmp[0] = _writeBuffer.Dequeue();
                tmp = tmp[1..];
            }

            Encrypt(span);
            stream.Write(span);
        }

        stream.Flush();
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();
}
