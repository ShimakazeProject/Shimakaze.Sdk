﻿using Shimakaze.Sdk;
using Shimakaze.Sdk.Graphic.Pal;

namespace Shimakaze.Sdk.Vpl;

/// <summary>
/// 体素文件调色板读取器
/// </summary>
/// <param name="stream">基础流</param>
/// <param name="skipPostprocess">
/// 跳过后处理 <br/>
/// pal文件中保存的颜色需要左移两位才能变成正常展示使用的颜色。<br/>
/// 设置为<see langword="true"/>则跳过左移处理。
/// </param>
/// <param name="leaveOpen"></param>
public sealed class VoxelPaletteReader(Stream stream, bool skipPostprocess = false, bool leaveOpen = false) : IDisposable, IAsyncDisposable
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

        using (PaletteReader reader = new(_disposable.Resource, Palette.DefaultColorCount, skipPostprocess, true))
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