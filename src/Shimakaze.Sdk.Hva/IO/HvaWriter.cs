using Shimakaze.Sdk.Hva;

namespace Shimakaze.Sdk.IO.Hva;

/// <summary>
/// HvaWriter
/// </summary>
public sealed class HvaWriter : IWriter<HvaFile>, IDisposable, IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private readonly byte[] _buffer = new byte[128];

    /// <summary>
    /// HvaWriter
    /// </summary>
    public HvaWriter(Stream stream, bool leaveOpen = false)
    {
        _stream = stream;
        _leaveOpen = leaveOpen;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_leaveOpen)
            _stream.Dispose();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (!_leaveOpen)
            await _stream.DisposeAsync();
    }

    /// <inheritdoc/>
    public void Write(in HvaFile value)
    {
        _stream.Write(value.Header);
        _stream.Write(value.SectionNames);
        foreach (var item in value.Frames)
            _stream.Write(item.Matrices);
    }
}