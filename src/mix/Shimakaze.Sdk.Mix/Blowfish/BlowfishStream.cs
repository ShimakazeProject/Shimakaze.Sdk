namespace Shimakaze.Sdk.Mix.Blowfish;
internal sealed class BlowfishStream(Stream stream, ReadOnlySpan<byte> key) : Stream
{
    private readonly Codec _codec = new(key);

    public override bool CanRead => stream.CanRead;
    public override bool CanSeek => false;
    public bool CanUnsafeSeek => stream.CanSeek;
    public override bool CanWrite => stream.CanWrite;
    public override long Length => stream.Length;

    public override long Position
    {
        get => stream.Position;
        set => Seek(value, SeekOrigin.Begin);
    }

    internal bool Encrypt(Span<byte> data)
    {
        if (data.Length is 0 || data.Length % 8 != 0)
            return false;

        for (var i = 0; i < data.Length; i += 8)
            _codec.Encrypt(data.Slice(i, 8));

        return true;
    }

    internal bool Decrypt(Span<byte> data)
    {
        if (data.Length is 0 || data.Length % 8 != 0)
            return false;

        for (var i = 0; i < data.Length; i += 8)
            _codec.Decrypt(data.Slice(i, 8));

        return true;
    }

    public override void Flush() => stream.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (count == 0)
            return 0;

        int aligned = (count + 7) / 8 * 8;
        byte[] temp = new byte[aligned];

        int read = stream.Read(temp, 0, aligned);
        if (read == 0)
            return 0;

        int decryptLength = read - (read % 8);
        if (decryptLength == 0)
            return 0;

        Decrypt(temp.AsSpan(0, decryptLength));

        int toCopy = Math.Min(count, decryptLength);
        Array.Copy(temp, 0, buffer, offset, toCopy);

        return toCopy;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        int aligned = (count + 7) / 8 * 8;
        Span<byte> temp = new byte[aligned];
        buffer.AsSpan(offset, count).CopyTo(temp);
        // 自动补0，不使用PKCS7

        Encrypt(temp);
        stream.Write(temp);
    }

    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotSupportedException("BlowfishStream does not support normal seeking");

    public long UnsafeSeek(long offset, SeekOrigin origin)
    {
        if (!CanUnsafeSeek)
            throw new NotSupportedException("Underlying stream does not support seeking");

        long newPosition = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => Length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };

        return stream.Seek(newPosition, SeekOrigin.Begin);
    }

    public override void SetLength(long value) => stream.SetLength(value);
}
