using Shimakaze.Sdk.Mix.Structs;

namespace Shimakaze.Sdk.Mix;

/// <summary>
/// Represents a single entry (file) within a <see cref="MixArchive"/>.
/// <br />
/// An entry can be backed by an in-memory <see cref="MemoryStream"/> (created via
/// <see cref="MixArchive.CreateEntry"/>) or a sub-range of a source stream (read from an existing archive).
/// </summary>
#pragma warning disable CA1001 // 具有可释放字段的类型应该是可释放的
public sealed class MixArchiveEntry
#pragma warning restore CA1001 // 具有可释放字段的类型应该是可释放的
{
    private readonly bool _inMemory;
#pragma warning disable IDISP008 // Don't assign member with injected and created disposables
    private readonly Stream _stream;
#pragma warning restore IDISP008 // Don't assign member with injected and created disposables
    private readonly long _offset;
    private string? _name;

    /// <summary>
    /// Gets the <see cref="MixArchive"/> that owns this entry.
    /// </summary>
    public MixArchive Archive { get; }

    /// <summary>
    /// Gets or sets the name of this entry.
    /// <br />
    /// Setting a non-<see langword="null"/> name recalculates <see cref="Id"/>
    /// using the archive's <see cref="IdCalculator"/>.
    /// </summary>
    public string? Name
    {
        get => _name;
        set
        {
            _name = value;
            if (value is not null)
                Id = Archive.IdCalculator(value, Archive.Encoding);
        }
    }

    /// <summary>
    /// Gets the file ID of this entry.
    /// <br />
    /// Derived from <see cref="Name"/> via the archive's <see cref="IdCalculator"/>.
    /// </summary>
    public uint Id { get; private set; }

    /// <summary>
    /// Gets the size of this entry's data in bytes.
    /// </summary>
    public int Size { get; }

    internal MixArchiveEntry(MixArchive archive, string? name)
    {
        _inMemory = true;
        _stream = new MemoryStream();
        _offset = 0;
        Id = 0;
        Size = 0;
        Archive = archive;
        Name = name;
    }

    internal MixArchiveEntry(MixArchive archive, Stream stream, long baseOffset, string? name, in MixEntry entry)
    {
        _inMemory = false;
        _stream = stream;
        _name = name;
        _offset = baseOffset + entry.Offset;
        Id = entry.Id;
        Size = entry.Size;
        Archive = archive;
    }

    /// <summary>
    /// Opens a <see cref="Stream"/> for reading the data of this entry.
    /// <br />
    /// For in-memory entries, returns a wrapped <see cref="MemoryStream"/> that can be read and written.
    /// <br />
    /// For stream-backed entries, returns a read-only sub-range view over the source stream.
    /// </summary>
    /// <returns>A readable stream positioned at the beginning of the entry data.</returns>
    public Stream Open()
    {
        if (_inMemory)
            return new StreamWrap(_stream);

        return new SubReadStream(_stream, _offset, Size)
        {
            Position = 0
        };
    }

    internal int GetDataSize() => _inMemory ? (int)_stream.Length : Size;

    internal void WriteDataTo(Stream destination)
    {
        if (_inMemory)
        {
            _stream.Position = 0;
            _stream.CopyTo(destination);
        }
        else
        {
            byte[] buffer = GC.AllocateUninitializedArray<byte>(Math.Min(Size, 8192));
            int remaining = Size;
            _stream.Seek(_offset, SeekOrigin.Begin);
            while (remaining > 0)
            {
                int toRead = Math.Min(remaining, buffer.Length);
                _stream.ReadExactly(buffer.AsSpan(0, toRead));
                destination.Write(buffer.AsSpan(0, toRead));
                remaining -= toRead;
            }
        }
    }

    internal async Task WriteDataToAsync(Stream destination, CancellationToken cancellationToken)
    {
        if (_inMemory)
        {
            _stream.Position = 0;
            await _stream.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            Memory<byte> buffer = GC.AllocateUninitializedArray<byte>(Math.Min(Size, 8192));
            int remaining = Size;
            _stream.Seek(_offset, SeekOrigin.Begin);
            while (remaining > 0)
            {
                int toRead = Math.Min(remaining, buffer.Length);
                int read = await _stream.ReadAsync(buffer[..toRead], cancellationToken).ConfigureAwait(false);
                await destination.WriteAsync(buffer[..read], cancellationToken).ConfigureAwait(false);
                remaining -= read;
            }
        }
    }
}
/// <summary>
/// A read-only <see cref="Stream"/> that represents a fixed sub-range of another stream.
/// </summary>
/// <param name="stream">The underlying stream.</param>
/// <param name="start">The absolute start position in the underlying stream.</param>
/// <param name="length">The length of the sub-range.</param>
file sealed class SubReadStream(Stream stream, long start, long length) : Stream
{
    private readonly Stream _stream = stream;
    private readonly long _start = start;
    private readonly long _length = length;

    public override bool CanRead => true;
    public override bool CanSeek => _stream.CanSeek;
    public override bool CanWrite => false;

    public override long Length => _length;

    public override long Position
    {
        get => _stream.Position - _start;
        set
        {
            if (value < 0 || value > _length)
                throw new ArgumentOutOfRangeException(nameof(value));

            _stream.Position = _start + value;
        }
    }

    public override void Flush() => _stream.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
        long remaining = _length - Position;
        if (remaining <= 0)
            return 0;

        if (count > remaining)
            count = (int)remaining;

        return _stream.Read(buffer, offset, count);
    }


#if NET5_0_OR_GREATER
    public override int Read(Span<byte> buffer)
    {
        long remaining = _length - Position;
        if (remaining <= 0)
            return 0;

        if (buffer.Length > remaining)
            buffer = buffer[..(int)remaining];

        return _stream.Read(buffer);
    }
#endif

    public override long Seek(long offset, SeekOrigin origin)
    {
        long newPos = origin switch
        {
            SeekOrigin.Begin => _start + offset,
            SeekOrigin.Current => _stream.Position + offset,
            SeekOrigin.End => _start + _length + offset,
            _ => throw new NotSupportedException(),
        };

        long rel = newPos - _start;

        if (rel < 0 || rel > _length)
            throw new IOException("Seek outside sub stream range.");

        _stream.Position = newPos;
        return rel;
    }

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();
}
/// <summary>
/// A simple wrapper <see cref="Stream"/> that delegates all operations to an inner stream.
/// <br />
/// Used to wrap in-memory entry streams so the caller always receives a disposable wrapper.
/// </summary>
/// <param name="inner">The inner stream to delegate to.</param>
file sealed class StreamWrap(Stream inner) : Stream
{
    public override bool CanRead => inner.CanRead;
    public override bool CanSeek => inner.CanSeek;
    public override bool CanWrite => inner.CanWrite;

    public override long Length => inner.Length;

    public override long Position
    {
        get => inner.Position;
        set => inner.Position = value;
    }

    public override void Flush() => inner.Flush();

    public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);

#if NET5_0_OR_GREATER
    public override int Read(Span<byte> buffer) => inner.Read(buffer);
#endif

    public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

    public override void SetLength(long value) => inner.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
}
