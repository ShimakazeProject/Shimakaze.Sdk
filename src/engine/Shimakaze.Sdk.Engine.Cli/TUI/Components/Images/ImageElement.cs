using System.Drawing;

using Shimakaze.Sdk.Engine.Common;

namespace Shimakaze.Sdk.Engine.Cli.TUI.Components.Images;

internal abstract class ImageElement : ITUIElement, IDisposable
{
    private bool _disposedValue;

    public virtual Image? Image { get; set; }

    public static ImageElement Create()
    {
        if (TerminalImageSupport.Kitty) return new KittyImage();
        if (TerminalImageSupport.ITerm) throw new NotSupportedException();
        if (TerminalImageSupport.Sixel) return new SixelImage();
        //if (TerminalImageSupport.GDI32) return new GDI32Image();
        //if (TerminalImageSupport.Unicode) return new UnicodeBlockImage() { Image = image };
        throw new NotSupportedException();
    }

    public abstract Size Measure(Size max);

    public abstract void OnRender(TextWriter writer, Size size);

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        _disposedValue = true;
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
