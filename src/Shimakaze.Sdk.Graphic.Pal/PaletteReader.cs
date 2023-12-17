namespace Shimakaze.Sdk.Graphic.Pal;

/// <summary>
/// 调色板读取器
/// </summary>
/// <param name="stream">基础流</param>
/// <param name="length">读取出的颜色数量</param>
/// <param name="skipPostprocess">
/// 跳过后处理 <br/>
/// pal文件中保存的颜色需要左移两位才能变成正常展示使用的颜色。<br/>
/// 设置为<see langword="true"/>则跳过左移处理。
/// </param>
/// <param name="leaveOpen"></param>
public sealed class PaletteReader(Stream stream, int length = Palette.DefaultColorCount, bool skipPostprocess = false, bool leaveOpen = false) : IDisposable, IAsyncDisposable
{
    private readonly DisposableObject<Stream> _disposable = new(stream, leaveOpen);

    /// <inheritdoc/>
    public void Dispose() => _disposable.Dispose();

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _disposable.DisposeAsync();

    /// <inheritdoc />
    public Palette Read()
    {
        Palette palette = new(length);
        _disposable.Resource.Read(palette.Colors);
        if (!skipPostprocess)
        {
            unsafe
            {
                fixed (void* ptr = palette.Colors)
                {
                    byte* p = (byte*)ptr;
                    for (var i = 0; i < length * 3; i++)
                    {
                        p[i] <<= 2;
                    }
                }
            }
        }
        return palette;
    }
}