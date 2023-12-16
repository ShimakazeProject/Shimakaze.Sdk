﻿using Shimakaze.Sdk.Graphic.Pal;

namespace Shimakaze.Sdk.Vpl;

/// <summary>
/// 体素调色板写入器
/// </summary>
/// <param name="stream"></param>
/// <param name="skipPreprocess">
/// 跳过预处理 <br/>
/// 正常展示使用的颜色需要右移两位才能变成pal文件中保存的颜色。<br/>
/// 设置为<see langword="true"/>则跳过右移处理。</param>
/// <param name="leaveOpen"></param>
public sealed class VoxelPaletteWriter(Stream stream, bool leaveOpen = false) : IDisposable, IAsyncDisposable
{
    private readonly DisposableObject<Stream> _disposable = new(stream, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();

    /// <inheritdoc />
    public void Write(VoxelPalette value, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        _disposable.Resource.Write(value.Header);

        using (PaletteWriter writer = new(_disposable, true, true))
            writer.Write(value.Palette);

        for (int i = 0; i < value.Sections.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((float)i / value.Sections.Length);

            unsafe
            {
                fixed (byte* ptr = value.Sections[i].Data)
                    _disposable.Resource.Write(new Span<byte>(ptr, 256));
            }
        }
    }
}