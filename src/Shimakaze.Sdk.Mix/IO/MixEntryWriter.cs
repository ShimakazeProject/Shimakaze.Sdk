using System.Runtime.InteropServices;

using Shimakaze.Sdk.Mix;

namespace Shimakaze.Sdk.IO.Mix;

/// <summary>
/// Mix Entry 读取器
/// </summary>
public sealed class MixEntryWriter : AsyncWriter<MixEntry>, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 当前文件个数
    /// </summary>
    private short _count;

    private bool _inited;

    /// <summary>
    /// 当前文件大小
    /// </summary>
    private int _size;

    /// <summary>
    /// 流开始的位置
    /// </summary>
    private long _start;

    /// <summary>
    /// 构造 Mix Entry 读取器
    /// </summary>
    /// <param name="stream"> 基础流 </param>
    /// <param name="leaveOpen"> 退出时是否保持流打开 </param>
    public MixEntryWriter(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
        if (!stream.CanSeek)
            throw new NotSupportedException("The Stream cannot support Seek.");
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        _start = BaseStream.Position;
        BaseStream.Seek(4 + 2 + 4, SeekOrigin.Current);

        _inited = true;
    }

    /// <inheritdoc />
    public void Write(in MixEntry value)
    {
        if (!_inited)
            Init();

        BaseStream.Write(value);

        _size = Math.Max(_size, value.Offset + value.Size);
        _count++;
    }

    /// <inheritdoc />
    public override async Task WriteAsync(MixEntry value, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        Write(value);
    }

    /// <summary>
    /// 写入元数据
    /// </summary>
    public void WriteMetadata()
    {
        long current = BaseStream.Position;
        BaseStream.Seek(_start, SeekOrigin.Begin);
        WriteMetadataDirect((int)MixFlag.NONE, new(_count, _size));
        BaseStream.Seek(current, SeekOrigin.Begin);
    }

    /// <summary>
    /// 直接写入元数据
    /// </summary>
    /// <param name="flag"> 标记 </param>
    /// <param name="metadata"> 元数据 </param>
    internal void WriteMetadataDirect(int flag, MixMetadata metadata)
    {
        BaseStream.Write(BitConverter.GetBytes(flag));
        unsafe
        {
            nint ptr = Marshal.AllocHGlobal(sizeof(MixMetadata));
            try
            {
                Marshal.StructureToPtr(metadata, ptr, true);
                BaseStream.Write(new Span<byte>((byte*)ptr, sizeof(MixMetadata)));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}