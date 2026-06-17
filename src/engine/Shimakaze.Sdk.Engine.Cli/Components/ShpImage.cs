using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

using Shimakaze.Sdk.Pal;
using Shimakaze.Sdk.Shp;

namespace Shimakaze.Sdk.Engine.Cli.Components;

internal sealed class ShpImage : IDisposable
{
    private readonly ShapeImage _shp;
    private readonly Palette _pal;
    private readonly SixelImage _sixel;
    private readonly int _half;

    private int _index;
    private Color[]? _cache;
    private bool _shadowUpdated;
    private bool _houseUpdated;
    private bool _disposedValue;


    public int Max { get; private set; }
    public int Index
    {
        get => _index;
        set
        {
            _index = int.Clamp(value, 0, Max);
            UpdateImage();
        }
    }

    public bool HasShadow
    {
        get;
        set
        {
            Max = value
                ? _half
                : _shp.Frames.Count;
            Max--;
            _index = int.Clamp(_index, 0, Max);
            field = value;
            UpdateImage();
        }
    }

    public Color? ShadowColor
    {
        get;
        set
        {
            _shadowUpdated = false;
            field = value;
            UpdateImage();
        }
    }

    public Color HouseColor
    {
        get;
        set
        {
            _houseUpdated = false;
            field = value;
            UpdateImage();
        }
    }

    public bool UseTransparent
    {
        get;
        set
        {
            field = value;
            UpdateImage();
        }
    } = true;

    public bool Center
    {
        get => _sixel.Center;
        set => _sixel.Center = value;
    }

    public ShpImage(ShapeImage shp, Palette pal)
    {
        _shp = shp;
        _pal = pal;
        _sixel = new();
        _half = _shp.Frames.Count / 2;
        Max = _shp.Frames.Count - 1;
        _houseUpdated = true;
        UpdateImage();
        HouseColor = _cache[16];
        _houseUpdated = true;
    }

    //protected override Measurement Measure(RenderOptions options, int maxWidth)
    //    => ((IRenderable)_sixel).Measure(options, maxWidth);

    public override string ToString() => _sixel.ToString();
    private void UpdateHouse(int index)
    {
        Debug.Assert(_cache is { Length: not 0 });
        _cache[index] = _cache[index].WithH(HouseColor.GetHue());
    }

    [MemberNotNull(nameof(_cache))]
    private void UpdateImage()
    {
        _cache ??= [.. _pal.Cast<DisplayColor>().Select(i => i.ToColor())];
        if (!_shadowUpdated)
        {
            _cache[1] = ShadowColor ?? _pal[1].AsDisplay().ToColor();

            _shadowUpdated = true;
        }

        if (!_houseUpdated)
        {
            UpdateHouse(16);
            UpdateHouse(17);
            UpdateHouse(18);
            UpdateHouse(19);
            UpdateHouse(20);
            UpdateHouse(21);
            UpdateHouse(22);
            UpdateHouse(23);
            UpdateHouse(24);
            UpdateHouse(25);
            UpdateHouse(26);
            UpdateHouse(27);
            UpdateHouse(28);
            UpdateHouse(29);
            UpdateHouse(30);
            UpdateHouse(31);

            _houseUpdated = true;
        }

        short[] indexes = GC.AllocateUninitializedArray<short>(_shp.Metadata.Width * _shp.Metadata.Height);
        Array.Fill(indexes, (short)(UseTransparent ? -1 : 0));

        if (HasShadow)
        {
            var i = Index + _half;
            Draw(_shp.Frames[i], indexes);
        }
        Draw(_shp.Frames[Index], indexes);

        _sixel.Width = _shp.Metadata.Width;
        _sixel.Height = _shp.Metadata.Height;
        _sixel.Palette = _cache;
        _sixel.Indexes = indexes;
    }

    private void Draw(ShapeImageFrame shapeImageFrame, short[] indexes)
    {
        for (ushort y = 0; y < shapeImageFrame.Metadata.Height; y++)
        {
            for (ushort x = 0; x < shapeImageFrame.Metadata.Width; x++)
            {
                int i = y * shapeImageFrame.Metadata.Width + x;
                var p = shapeImageFrame.Indexes.Span[i];
                if (p is not 0)
                {
                    int g = (shapeImageFrame.Metadata.Y + y) * _shp.Metadata.Width + shapeImageFrame.Metadata.X + x;
                    indexes[g] = p;
                }
            }
        }
    }

    private void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            _sixel.Dispose();
        }

        _disposedValue = true;
    }


    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        //GC.SuppressFinalize(this);
    }
}
