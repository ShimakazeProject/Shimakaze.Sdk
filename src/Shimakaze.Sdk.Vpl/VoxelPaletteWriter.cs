using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vpl;

/// <summary>
/// 体素调色板写入器
/// </summary>
public sealed class VoxelPaletteWriter
{
    /// <summary>
    /// 写入体素调色板
    /// </summary>
    /// <param name="value">体素调色板</param>
    /// <param name="stream">流</param>
    /// <param name="progress"></param>
    /// <param name="cancellationToken"></param>
    public static void Write(VoxelPalette value, Stream stream, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        stream.Write(value.Header);

        PaletteWriter.Write(value.Palette, stream, skipPreprocess: true);

        for (int i = 0; i < value.Sections.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((float)i / value.Sections.Length);

            unsafe
            {
                fixed (byte* ptr = value.Sections[i].Data)
                    stream.Write(new Span<byte>(ptr, 256));
            }
        }
    }
}