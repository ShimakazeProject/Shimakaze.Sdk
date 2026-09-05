using System.Text;

using Shimakaze.Sdk.Mix.Blowfish;
using Shimakaze.Sdk.Mix.Structs;

namespace Shimakaze.Sdk.Mix;

/// <summary>
/// Represents a MIX archive that can read, create, modify, and save entries.
/// <br />
/// Supports optional Blowfish encryption for the header via <see cref="MixTag.ENCRYPTED"/>.
/// </summary>
public sealed class MixArchive : IDisposable, IAsyncDisposable
{
    private readonly long _streamStart;
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private readonly bool _hasNoFlag;
    private readonly MixArchiveMode _mode;
    private readonly List<MixArchiveEntry> _entries = [];
    private readonly List<MemoryStream> _ms = [];
    internal readonly Encoding? Encoding;
    internal readonly IdCalculator IdCalculator;

    //private bool _modified;
    private bool _disposedValue;

    /// <summary>
    /// Gets the collection of entries contained in this archive.
    /// </summary>
    public IReadOnlyList<MixArchiveEntry> Entries => _entries;

    /// <summary>
    /// Opens an existing MIX archive from the specified stream for reading.
    /// </summary>
    /// <param name="stream">The stream containing the MIX archive data.</param>
    /// <param name="idCalculator">
    /// The ID calculator to use for entry name hashing.
    /// <br />
    /// Defaults to <see cref="IdCalculators.TSIdCalculator"/>.
    /// </param>
    /// <param name="encoding">
    /// The encoding to use for entry names.
    /// <br />
    /// Defaults to the system's active code page.
    /// </param>
    /// <param name="leaveOpen">
    /// <see langword="true"/> to leave the stream open after the <see cref="MixArchive"/> is disposed.
    /// </param>
    /// <param name="hasNoFlag">
    /// <see langword="true"/> if the archive has no header flag (e.g. old CnC1/RA1 MIX files).
    /// </param>
    /// <returns>A <see cref="MixArchive"/> opened for reading.</returns>
    public static MixArchive Open(Stream stream, IdCalculator? idCalculator = default, Encoding? encoding = null, bool leaveOpen = false, bool hasNoFlag = false)
        => new(stream, MixArchiveMode.Read, idCalculator, encoding, leaveOpen, hasNoFlag);

    /// <summary>
    /// Creates a new empty MIX archive backed by an in-memory stream.
    /// </summary>
    /// <param name="idCalculator">
    /// The ID calculator to use for entry name hashing.
    /// <br />
    /// Defaults to <see cref="IdCalculators.TSIdCalculator"/>.
    /// </param>
    /// <param name="encoding">
    /// The encoding to use for entry names.
    /// <br />
    /// Defaults to the system's active code page.
    /// </param>
    /// <param name="hasNoFlag">
    /// <see langword="true"/> if the archive has no header flag (e.g. old CnC1/RA1 MIX files).
    /// </param>
    /// <returns>A new empty <see cref="MixArchive"/>.</returns>
    public static MixArchive Create(IdCalculator? idCalculator = default, Encoding? encoding = null, bool hasNoFlag = false)
        => new(new MemoryStream(), MixArchiveMode.Create, idCalculator, encoding, hasNoFlag: hasNoFlag);

    private MixArchive(Stream stream, MixArchiveMode mode, IdCalculator? idCalculator = default, Encoding? encoding = null, bool leaveOpen = false, bool hasNoFlag = false)
    {
        _stream = stream;
        _streamStart = _stream.Position;
        _leaveOpen = leaveOpen;
        _mode = mode;
        Encoding = encoding ?? Encoding.GetEncoding(0);
        IdCalculator = idCalculator ?? IdCalculators.TSIdCalculator;
        _hasNoFlag = hasNoFlag;
        if (mode is MixArchiveMode.Read or MixArchiveMode.Update)
            Initialize();
    }

    private void Initialize()
    {
        var flag = MixTag.NONE;
        bool isEncrypted = false;

        using var decryptedStream = new Func<BlowfishStream?>(() =>
        {
            if (_hasNoFlag)
                return null;

            _stream.Read(out flag);
            if (!flag.HasFlag(MixTag.ENCRYPTED))
                return null;

            isEncrypted = true;
            Span<byte> keySource = stackalloc byte[80];
            _stream.ReadExactly(keySource);

            Span<byte> key = stackalloc byte[56];
            WSKey.Decrypt(keySource, key);

            return new BlowfishStream(_stream, key, true);
        })();

        var stream = decryptedStream ?? _stream;

        stream.Read(out MixMetadata info);

        int flagSize = _hasNoFlag ? 0 : sizeof(MixTag);
        int keySize = isEncrypted ? 80 : 0;

        int tableSize = 6 + 12 * info.Files;
        int tableAligned = isEncrypted
            ? ((tableSize + 7) & ~7)
            : tableSize;

        int bodyOffset = flagSize + keySize + tableAligned;

        var entries = GC.AllocateUninitializedArray<MixEntry>(info.Files);
        stream.Read(entries);
        _entries.Clear();
        _entries.EnsureCapacity(entries.Length);
        foreach (ref readonly var item in entries.AsSpan())
            _entries.Add(new MixArchiveEntry(this, _stream, _streamStart + bodyOffset, null, item));
    }

    /// <summary>
    /// Creates a new in-memory entry and adds it to the archive.
    /// </summary>
    /// <param name="name">
    /// The name of the entry.
    /// <br />
    /// When not <see langword="null"/>, the entry's <see cref="MixArchiveEntry.Id"/> is computed
    /// via the archive's <see cref="IdCalculator"/>.
    /// </param>
    /// <returns>The newly created entry.</returns>
    public MixArchiveEntry CreateEntry(string? name)
    {
        //_modified = true;
        MemoryStream ms = new();
        _ms.Add(ms);
        MixArchiveEntry entry = new(this, ms, name);
        _entries.Add(entry);
        return entry;
    }

    /// <summary>
    /// Removes the specified entry from the archive.
    /// </summary>
    /// <param name="entry">The entry to remove.</param>
    /// <returns><see langword="true"/> if the entry was successfully removed; otherwise, <see langword="false"/>.</returns>
    public bool DeleteEntry(MixArchiveEntry entry) =>
        //_modified = true;
        _entries.Remove(entry);

    /// <summary>
    /// Writes the entire archive to the specified destination stream.
    /// </summary>
    /// <param name="destination">The stream to write the archive to.</param>
    /// <param name="flag">
    /// The <see cref="MixTag"/> flags for the archive header.
    /// <br />
    /// Pass <see langword="null"/> to omit the flag (for flagless archives).
    /// <br />
    /// Pass <see cref="MixTag.ENCRYPTED"/> to encrypt the header with Blowfish.
    /// </param>
    /// <param name="key">The Blowfish key. Required when <paramref name="flag"/> has <see cref="MixTag.ENCRYPTED"/>.</param>
    /// <param name="alignBody4">
    /// When <see langword="true"/>, each entry's body data is padded to a 4-byte boundary.
    /// </param>
    public void SaveTo(
        Stream destination,
        MixTag? flag = MixTag.NONE,
        ReadOnlyMemory<byte> key = default,
        bool alignBody4 = false)
    {
        int count = _entries.Count;
        if (count > short.MaxValue)
            throw new InvalidOperationException("Archive contains too many entries.");

        var entries = count <= 128 ? stackalloc MixEntry[count] : new MixEntry[count];

        int bodySize = 0;

        for (int i = 0; i < count; i++)
        {
            int size = _entries[i].GetDataSize();
            int writeSize = alignBody4 ? Align4(size) : size;

            entries[i] = new MixEntry(_entries[i].Id, bodySize, size);
            bodySize += writeSize;
        }

        MixMetadata metadata = new((short)count, bodySize);

        WriteHeader(destination, metadata, entries, flag, key);

        for (int i = 0; i < count; i++)
        {
            int size = _entries[i].GetDataSize();

            _entries[i].WriteDataTo(destination);

            if (alignBody4)
            {
                int pad = Align4(size) - size;
                for (int j = 0; j < pad; j++)
                    destination.Write(0);
            }
        }
    }
    /// <summary>
    /// Asynchronously writes the entire archive to the specified destination stream.
    /// </summary>
    /// <param name="destination">The stream to write the archive to.</param>
    /// <param name="flag">
    /// The <see cref="MixTag"/> flags for the archive header.
    /// <br />
    /// Pass <see langword="null"/> to omit the flag (for flagless archives).
    /// <br />
    /// Pass <see cref="MixTag.ENCRYPTED"/> to encrypt the header with Blowfish.
    /// </param>
    /// <param name="key">The Blowfish key. Required when <paramref name="flag"/> has <see cref="MixTag.ENCRYPTED"/>.</param>
    /// <param name="alignBody4">
    /// When <see langword="true"/>, each entry's body data is padded to a 4-byte boundary.
    /// </param>
    /// <param name="progress">An optional progress reporter that receives the fraction of bytes written.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveToAsync(
        Stream destination,
        MixTag? flag = MixTag.NONE,
        byte[]? key = default,
        bool alignBody4 = false,
        IProgress<float>? progress = null,
        CancellationToken cancellationToken = default)
    {

        int count = _entries.Count;
        if (count > short.MaxValue)
            throw new InvalidOperationException("Archive contains too many entries.");

        var entries = GC.AllocateUninitializedArray<MixEntry>(count);

        int bodySize = 0;

        for (int i = 0; i < count; i++)
        {
            int size = _entries[i].GetDataSize();
            int writeSize = alignBody4 ? Align4(size) : size;

            entries[i] = new MixEntry(_entries[i].Id, bodySize, size);
            bodySize += writeSize;
        }

        MixMetadata metadata = new((short)count, bodySize);

        WriteHeader(destination, metadata, entries, flag, key ?? []);

        long totalBytes = bodySize;
        long bytesWritten = 0;

        for (int i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int size = _entries[i].GetDataSize();

            await _entries[i]
                .WriteDataToAsync(destination, cancellationToken)
                .ConfigureAwait(false);

            if (alignBody4)
            {
                int pad = Align4(size) - size;
                for (int j = 0; j < pad; j++)
                    destination.Write(0);
            }

            bytesWritten += alignBody4 ? Align4(size) : size;

            progress?.Report(totalBytes > 0
                ? (float)bytesWritten / totalBytes
                : 1f);
        }
    }

    private static int Align4(int v) => (v + 3) & ~3;

    private static void WriteHeader(Stream destination, in MixMetadata metadata, ReadOnlySpan<MixEntry> entries, MixTag? flag, ReadOnlyMemory<byte> key)
    {
        using var encryptedStream = new Func<BlowfishStream?>(() =>
        {
            if (flag is null)
                return null;

            destination.Write(flag.Value);
            if (!flag.Value.HasFlag(MixTag.ENCRYPTED))
                return null;

            if (key.IsEmpty)
                throw new ArgumentException("Key cannot be empty.", nameof(key));

            return new BlowfishStream(destination, key.Span, true);
        })();

        var headerStream = encryptedStream ?? destination;
        headerStream.Write(metadata);
        headerStream.Write(entries);
        headerStream.Flush();
    }

    private void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            if (!_leaveOpen)
                _stream.Dispose();
        }

        _disposedValue = true;
    }
    private async ValueTask DisposeAsyncCore()
    {
        if (!_leaveOpen)
            await _stream.DisposeAsync();
    }


    /// <inheritdoc/>
    public void Dispose() => Dispose(disposing: true);

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(false);
    }
}
