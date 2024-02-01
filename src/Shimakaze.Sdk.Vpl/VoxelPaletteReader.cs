using Shimakaze.Sdk;
using Shimakaze.Sdk.Pal;

namespace Shimakaze.Sdk.Vpl;

/// <summary>
/// 基础流读取器
/// </summary>
public sealed class VoxelPaletteReader
{
    /// <summary>
    /// 体素文件调色板
    /// </summary>
    /// <param name="stream">流</param>
    /// <param name="progress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static VoxelPalette Read(Stream stream, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        VoxelPalette vpl = new();

        stream.Read(out vpl.InternalHeader);

        vpl.Palette = PaletteReader.Read(stream, skipPostprocess: true);

        vpl.Sections = new VoxelPaletteSection[vpl.Header.SectionCount];
        for (int i = 0; i < vpl.Sections.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((float)i / vpl.Sections.Length);

            vpl.Sections[i] = new();
            unsafe
            {
                fixed (byte* p = vpl.Sections[i].Data)
                    stream.Read(new Span<byte>(p, 256));
            }
        }

        return vpl;
    }
}