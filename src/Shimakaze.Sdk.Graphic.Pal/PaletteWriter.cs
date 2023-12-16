using Shimakaze.Sdk.Graphic.Pixel;

namespace Shimakaze.Sdk.Graphic.Pal;

/// <summary>
/// 调色板写入器
/// </summary>
/// <param name="stream"></param>
/// <param name="skipPreprocess">
/// 跳过预处理 <br/>
/// 正常展示使用的颜色需要右移两位才能变成pal文件中保存的颜色。<br/>
/// 设置为<see langword="true"/>则跳过右移处理。</param>
/// <param name="leaveOpen"></param>
public sealed class PaletteWriter(Stream stream, bool skipPreprocess = false, bool leaveOpen = false) : IDisposable, IAsyncDisposable
{
    private readonly DisposableObject<Stream> _disposable = new(stream, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();

    /// <summary>
    /// 写入调色板
    /// </summary>
    /// <param name="value"></param>
    public void Write(in Palette value)
    {
        Rgb24[] palette = value.Colors;
        if (!skipPreprocess)
        {
            palette = [.. palette];
            unsafe
            {
                fixed (void* ptr = palette)
                {
                    byte* p = (byte*)ptr;
                    for (var i = 0; i < palette.Length * 3; i++)
                    {
                        p[i] >>= 2;
                    }
                }
            }
        }
        _disposable.Resource.Write(palette);
    }
}