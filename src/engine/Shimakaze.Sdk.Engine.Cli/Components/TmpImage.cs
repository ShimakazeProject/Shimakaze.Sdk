using System.Drawing;

using Shimakaze.Sdk.Engine.Tmp;
using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Tmp;

namespace Shimakaze.Sdk.Engine.Cli.Components;

internal sealed class TmpImage : IDisposable
{
    private readonly SixelImage _sixel = new();
    private bool _disposedValue;

    public TmpImage(TemplateFile template, Palette palette)
    {
        TmpRenderer renderer = new(template, palette);
        var image = renderer.Render();
        SetImage(image);
    }

    public bool Center
    {
        get => _sixel.Center;
        set => _sixel.Center = value;
    }

    private void SetImage(Image image)
    {
        // 统计实际用到的颜色（去重），构建 Sixel 调色板和索引数组
        Dictionary<BGRA32, byte> colorMap = [];
        short[] indexes = GC.AllocateUninitializedArray<short>(image.Pixels.Length);
        Array.Fill<short>(indexes, -1);

        var pixels = image.Pixels.AsSpan();
        for (int i = 0; i < pixels.Length; i++)
        {
            var c = pixels[i];
            if (c.A == 0)
                continue;
            
            if (!colorMap.TryGetValue(c, out byte idx))
            {
                idx = (byte)colorMap.Count;
                colorMap[c] = idx;
            }
            indexes[i] = idx;
        }

        // 将去重后的颜色转为 Color[]
        Color[] palette = new Color[colorMap.Count];
        foreach (var (color, index) in colorMap)
            palette[index] = Color.FromArgb(color.R, color.G, color.B);

        _sixel.Width = image.Width;
        _sixel.Height = image.Height;
        _sixel.Palette = palette;
        _sixel.Indexes = indexes;
    }

    public override string ToString() => _sixel.ToString();

    private void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
            _sixel.Dispose();

        _disposedValue = true;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
    }
}
