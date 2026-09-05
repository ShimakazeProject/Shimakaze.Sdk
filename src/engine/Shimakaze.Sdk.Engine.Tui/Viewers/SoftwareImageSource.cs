using System.Runtime.InteropServices;

using Shimakaze.Sdk.Engine.Common;

using XenoAtom.Terminal.Graphics;

namespace Shimakaze.Sdk.Engine.Tui.Viewers;

/// <summary>
/// 基于 <see cref="SoftwareImage"/> 的实时图像源：调用 <see cref="Publish"/> 即通知查看器刷新，无需更换 Source 对象。
/// </summary>
internal sealed class SoftwareImageSource : TerminalImageSource, ITerminalRealtimeImageSource
{
    private readonly Lock _sync = new();
    private byte[] _bytes = [];
    private int _width = 1;
    private int _height = 1;
    private long _version;

    /// <inheritdoc />
    public event EventHandler<TerminalImageFrameAvailableEventArgs>? FrameAvailable;

    /// <inheritdoc />
    public TimeSpan MinimumFrameInterval => TimeSpan.Zero;

    /// <inheritdoc />
    public long Version => Interlocked.Read(ref _version);

    /// <summary>
    /// 发布一帧新图像，通知 <see cref="Image"/> 重新呈现。
    /// </summary>
    /// <param name="image">新的软件图像。</param>
    public void Publish(SoftwareImage image)
    {
        TerminalImageFrameAvailableEventArgs args;
        lock (_sync)
        {
            _width = image.Width;
            _height = image.Height;
            _bytes = MemoryMarshal.AsBytes(image.Pixels.AsSpan()).ToArray();
            long v = Interlocked.Increment(ref _version);
            args = new(v, TimeSpan.Zero);
        }

        FrameAvailable?.Invoke(this, args);
    }

    /// <summary>
    /// 不改变像素，仅递增版本号以触发一次重新呈现，用于在文本重绘后把图像保持在最上层。
    /// </summary>
    public void Touch()
    {
        long version;
        lock (_sync)
        {
            version = Interlocked.Increment(ref _version);
        }

        FrameAvailable?.Invoke(this, new(version, TimeSpan.Zero));
    }

    /// <inheritdoc />
    public override ValueTask<TerminalImageFrame?> GetFrameAsync(TerminalImageFrameRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        byte[] bytes;
        int width;
        int height;
        long version;
        lock (_sync)
        {
            bytes = _bytes;
            width = _width;
            height = _height;
            version = _version;
        }

        return ValueTask.FromResult<TerminalImageFrame?>(new()
        {
            Format = TerminalImageFormat.RawRgba32,
            Data = bytes,
            PixelWidth = width,
            PixelHeight = height,
            SourceId = "shimakaze-sdk-viewer",
            Version = version,
            Timestamp = request.Timestamp ?? TimeSpan.Zero,
        });
    }

    /// <inheritdoc />
    public ValueTask<TerminalImageFrame?> GetLatestFrameAsync(TerminalImageFrameRequest request, CancellationToken cancellationToken = default)
        => GetFrameAsync(request, cancellationToken);

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
