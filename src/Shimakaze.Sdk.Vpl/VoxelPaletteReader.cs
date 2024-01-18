using Shimakaze.Sdk;
using Shimakaze.Sdk.Graphic.Pal;

namespace Shimakaze.Sdk.Vpl;

/// <summary>
/// 体素文件调色板读取器
/// </summary>
/// <param name="stream">基础流</param>
/// <param name="leaveOpen"></param>
public sealed class VoxelPaletteReader(Stream stream, bool leaveOpen = false) : IDisposable, IAsyncDisposable
{
    private readonly DisposableObject<Stream> _disposable = new(stream, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();

    /// <inheritdoc />
    public VoxelPalette Read(IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        VoxelPalette vpl = new();

        _disposable.Resource.Read(out vpl.InternalHeader);

        using (PaletteReader reader = new(stream, skipPostprocess: true, leaveOpen: true))
            vpl.Palette = reader.Read();

        vpl.Sections = new VoxelPaletteSection[vpl.Header.SectionCount];
        for (int i = 0; i < vpl.Sections.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((float)i / vpl.Sections.Length);

            vpl.Sections[i] = new();
            unsafe
            {
                fixed (byte* p = vpl.Sections[i].Data)
                    _disposable.Resource.Read(new Span<byte>(p, 256));
            }
        }

        return vpl;
    }
}