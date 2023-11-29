using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.IO.Pal;

/// <summary>
/// PaletteWriter
/// </summary>
public sealed class PaletteWriter : AsyncWriter<Palette>, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// PaletteWriter
    /// </summary>
    public PaletteWriter(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
    }

    /// <inheritdoc />
    public void Write(in Palette value)
    {
        BaseStream.Write(value.Colors);
    }

    /// <inheritdoc />
    public override async Task WriteAsync(Palette value, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        Write(value);
    }
}