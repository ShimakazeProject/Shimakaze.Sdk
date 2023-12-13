namespace Shimakaze.Sdk.Hva;

/// <summary>
/// HvaReader
/// </summary>
public sealed class HvaReader(Stream stream, bool leaveOpen = false) : AsyncReader<HvaFile>(stream, leaveOpen), IDisposable, IAsyncDisposable
{

    /// <inheritdoc />
    public override async Task<HvaFile> ReadAsync(IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        HvaFile hva = new();

        BaseStream.Read(out hva.InternalHeader);

        hva.SectionNames = new Int128[hva.Header.NumSections];
        BaseStream.Read(hva.SectionNames);

        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report(1f / 3);

        await Task.Yield();

        hva.Frames = new HvaFrame[hva.Header.NumFrames];
        for (int i = 0; i < hva.Frames.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(1f / 3 + 2f / 3 * ((float)i / hva.Frames.Length));

            hva.Frames[i].Matrices = new HvaMatrix[hva.Header.NumSections];
            BaseStream.Read(hva.Frames[i].Matrices);
        }

        return hva;
    }
}