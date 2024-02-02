using System;
using System.IO;

namespace Shimakaze.Sdk.Hva;

/// <summary>
/// HvaReader
/// </summary>
public sealed class HvaReader(Stream stream, bool leaveOpen = false) : IDisposable, IAsyncDisposable
{
    private readonly DisposableObject<Stream> _disposable = new(stream, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();

    /// <inheritdoc />
    public async Task<HvaFile> ReadAsync(IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        HvaFile hva = new();

        _disposable.Resource.Read(out hva.InternalHeader);

        hva.SectionNames = new HvaSectionName[hva.Header.NumSections];
        _disposable.Resource.Read(hva.SectionNames);

        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report(1f / 3);

        await Task.Yield();

        hva.Frames = new HvaFrame[hva.Header.NumFrames];
        for (int i = 0; i < hva.Frames.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            hva.Frames[i] ??= new();
            progress?.Report(1f / 3 + 2f / 3 * ((float)i / hva.Frames.Length));

            hva.Frames[i].Matrices = new HvaMatrix[hva.Header.NumSections];
            _disposable.Resource.Read(hva.Frames[i].Matrices);
        }

        return hva;
    }
}