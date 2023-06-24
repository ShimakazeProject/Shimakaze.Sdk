using Shimakaze.Sdk.Hva;

namespace Shimakaze.Sdk.IO.Hva;

/// <summary>
/// HvaReader
/// </summary>
public sealed class HvaReader : IReader<HvaFile>, IDisposable, IAsyncDisposable
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private readonly byte[] _buffer = new byte[128];

    /// <summary>
    /// HvaReader
    /// </summary>

    public HvaReader(Stream stream, bool leaveOpen = false)
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
    public HvaFile Read()
    {
        HvaFile hva = new();

        _stream.Read(_buffer, out hva.Header);

        hva.SectionNames = new Int128[hva.Header.NumSections];
        _stream.Read(_buffer, hva.SectionNames);

        hva.Frames = new HvaFrame[hva.Header.NumFrames];
        for (int i = 0; i < hva.Frames.Length; i++)
        {
            hva.Frames[i].Matrices = new HvaMatrix[hva.Header.NumSections];
            _stream.Read(_buffer, hva.Frames[i].Matrices);
        }

        return hva;
    }
}
