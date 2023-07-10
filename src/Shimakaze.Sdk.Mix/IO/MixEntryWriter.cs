using System.Runtime.InteropServices;

using Shimakaze.Sdk.Mix;

namespace Shimakaze.Sdk.IO.Mix;

/// <summary>
/// Mix Entry ��ȡ��
/// </summary>
public sealed class MixEntryWriter : AsyncWriter<MixEntry>, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// ��ǰ�ļ�����
    /// </summary>
    private short _count;

    private bool _inited;

    /// <summary>
    /// ��ǰ�ļ���С
    /// </summary>
    private int _size;

    /// <summary>
    /// ����ʼ��λ��
    /// </summary>
    private long _start;

    /// <summary>
    /// ���� Mix Entry ��ȡ��
    /// </summary>
    /// <param name="stream"> ������ </param>
    /// <param name="leaveOpen"> �˳�ʱ�Ƿ񱣳����� </param>
    public MixEntryWriter(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
        if (!stream.CanSeek)
            throw new NotSupportedException("The Stream cannot support Seek.");
    }

    /// <summary>
    /// ��ʼ��
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
    /// д��Ԫ����
    /// </summary>
    public void WriteMetadata()
    {
        long current = BaseStream.Position;
        BaseStream.Seek(_start, SeekOrigin.Begin);
        WriteMetadataDirect((int)MixFlag.NONE, new(_count, _size));
        BaseStream.Seek(current, SeekOrigin.Begin);
    }

    /// <summary>
    /// ֱ��д��Ԫ����
    /// </summary>
    /// <param name="flag"> ��� </param>
    /// <param name="metadata"> Ԫ���� </param>
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