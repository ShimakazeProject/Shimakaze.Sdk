using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;

using Shimakaze.Sdk.Engine.Common;
using Shimakaze.Sdk.Engine.Common.Pixels;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Shimakaze.Sdk.Engine.Cli.TUI.Components.Images;

internal sealed unsafe class GDI32Image : TrueImageElement
{
    private readonly HWND _hWnd = PInvoke.GetConsoleWindow();
    private HDC _hMemDC;
    private void* _framebuffer;
    private DeleteObjectSafeHandle? _hBmp;
    private HGDIOBJ _hOldBmp;

    private Size _size;
    public override Image? Image
    {
        get => base.Image;
        set
        {
            if (value is null)
            {
                base.Image = value;
                return;
            }
            Debug.Assert(TerminalImageSupport.GDI32);
            var image = value.ToSoftware();

            if (image.Width != _size.Width || image.Height != _size.Height)
            {
                var hdc = PInvoke.GetDC(_hWnd);

                if (_hMemDC.IsNull)
                {
                    _hMemDC = PInvoke.CreateCompatibleDC(hdc);
                    Debug.Assert(!_hMemDC.IsNull);
                }

                if (_framebuffer is null || _hBmp is not { IsInvalid: false })
                {
                    BITMAPINFO bmi = new()
                    {
                        bmiHeader =
                        {
                            biSize = (uint)Unsafe.SizeOf<BITMAPINFOHEADER>(),
                            biWidth = image.Width,
                            biHeight = -image.Height,
                            biPlanes = 1,
                            biBitCount = 32,
                            biCompression = (uint)BI_COMPRESSION.BI_RGB,
                        }
                    };

                    _hBmp?.Dispose();
                    _hBmp = PInvoke.CreateDIBSection(_hMemDC, &bmi, DIB_USAGE.DIB_RGB_COLORS, out _framebuffer, null, 0);
                    Debug.Assert(!_hBmp.IsInvalid);
                }

                if (!_hOldBmp.IsNull)
                {
                    _hOldBmp = PInvoke.SelectObject(_hMemDC, _hOldBmp);
                    PInvoke.DeleteObject(_hOldBmp);
                    _hOldBmp = HGDIOBJ.Null;
                }
                _hOldBmp = PInvoke.SelectObject(_hMemDC, new HGDIOBJ(_hBmp.DangerousGetHandle()));

                _ = PInvoke.ReleaseDC(_hWnd, hdc);
                _size = new(image.Width, image.Height);
            }

            Span<BGRA32> framebuffer = new(_framebuffer, image.Width * image.Height);
            image.Pixels.CopyTo(framebuffer);

            base.Image = image;
        }
    }

    protected override void OnTrueRender(TextWriter writer, Size px)
    {
        if (Image is null)
            return;
        Debug.Assert(TerminalImageSupport.GDI32);

        int startX, startY;
        {
            var (l, t) = Console.GetCursorPosition();
            l -= Console.WindowLeft;
            t -= Console.WindowTop;
            (startX, startY) = (l * TerminalImageSupport.CellSize.Width, t * TerminalImageSupport.CellSize.Height);
        }
        var width = px.Width;
        var height = px.Height;

        var hdc = PInvoke.GetDC(_hWnd);
        PInvoke.BitBlt(hdc, 0, 0, width, height, _hMemDC, 0, 0, ROP_CODE.SRCCOPY);
        _ = PInvoke.ReleaseDC(_hWnd, hdc);

        Debug.Assert(true);
    }

    protected override void Dispose(bool disposing)
    {
        Debug.Assert(TerminalImageSupport.GDI32);
        if (disposing)
        {
            _hBmp?.Dispose();
        }

        if (!_hMemDC.IsNull)
        {
            if (!_hOldBmp.IsNull)
            {
                _hOldBmp = PInvoke.SelectObject(_hMemDC, _hOldBmp);
                PInvoke.DeleteObject(_hOldBmp);
                _hOldBmp = HGDIOBJ.Null;
            }
            PInvoke.DeleteDC(_hMemDC);
        }

        base.Dispose(disposing);
    }
}
