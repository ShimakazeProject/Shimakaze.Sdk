using Shimakaze.Sdk.Hva;

namespace Shimakaze.Sdk.IO.Hva;

/// <summary>
/// HvaWriter
/// </summary>
public sealed class HvaWriter : AsyncWriter<HvaFile>, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// HvaWriter
    /// </summary>
    public HvaWriter(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
    }

    /// <inheritdoc />
    public override async Task WriteAsync(HvaFile value, IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        BaseStream.Write(value.Header);
        BaseStream.Write(value.SectionNames);

        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report(1f / 3);

        await Task.Yield();

        for (int i = 0; i < value.Frames.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(1f / 3 + (2f / 3) * ((float)i / value.Frames.Length));

            HvaFrame item = value.Frames[i];
            BaseStream.Write(item.Matrices);
        }
    }
}