using System.Runtime.InteropServices;

using Shimakaze.Sdk.Mix;

namespace Shimakaze.Sdk.IO.Mix;
/// <summary>
/// Mix Entry ��ȡ��
/// </summary>
public sealed class MixEntryWriter : IWriter<MixEntry>, IDisposable, IAsyncDisposable
{
    private bool _inited;
    private readonly bool _leaveOpen;

    /// <summary>
    /// ��ǰ�ļ�����
    /// </summary>
    private short _count;
    /// <summary>
    /// ��ǰ�ļ���С
    /// </summary>
    private int _size;
    /// <summary>
    /// ����ʼ��λ��
    /// </summary>
    private long _start;

    /// <summary>
    /// ������
    /// </summary>
    public Stream BaseStream { get; }

    /// <summary>
    /// ���� Mix Entry ��ȡ��
    /// </summary>
    /// <param name="baseStream">������</param>
    /// <param name="leaveOpen">�˳�ʱ�Ƿ񱣳�����</param>
    public MixEntryWriter(Stream baseStream, bool leaveOpen = false)
    {
        if (!baseStream.CanSeek)
            throw new NotSupportedException("The Stream cannot support Seek.");

        BaseStream = baseStream;
        _leaveOpen = leaveOpen;
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

    /// <inheritdoc/>
    public void Write(in MixEntry value)
    {
        if (!_inited)
            Init();

        BaseStream.Write(value);

        _size = Math.Max(_size, value.Offset + value.Size);
        _count++;
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
    /// <param name="flag">���</param>
    /// <param name="metadata">Ԫ����</param>
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


    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_leaveOpen)
            BaseStream.Dispose();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (!_leaveOpen)
            await BaseStream.DisposeAsync();
    }
}
