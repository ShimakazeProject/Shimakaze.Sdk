using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using Shimakaze.Sdk.Mix;

namespace Shimakaze.Sdk.IO.Mix;
/// <summary>
/// Mix Entry ��ȡ��
/// </summary>
public class MixEntryWriter : IWriter<MixEntry>, IAsyncWriter<MixEntry, Task>, IDisposable, IAsyncDisposable
{
    private bool _inited;
    private bool _disposedValue;
    private readonly bool _leaveOpen;
    private readonly byte[] _buffer;

    /// <summary>
    /// ��ǰ�ļ�����
    /// </summary>
    protected short _count;
    /// <summary>
    /// ��ǰ�ļ���С
    /// </summary>
    protected int _size;
    /// <summary>
    /// ����ʼ��λ��
    /// </summary>
    protected long _start;

    /// <summary>
    /// ������
    /// </summary>
    public Stream BaseStream { get; }

    /// <summary>
    /// ���� Mix Entry ��ȡ��
    /// </summary>
    /// <param name="baseStream">������</param>
    /// <param name="leaveOpen">�˳�ʱ�Ƿ񱣳�����</param>
    /// <param name="buffer">��������С</param>
    public MixEntryWriter(Stream baseStream, bool leaveOpen = false, byte[]? buffer = default)
    {
        _buffer = buffer ?? new byte[12];

        if (!baseStream.CanSeek)
            throw new NotSupportedException("The Stream cannot support Seek.");

        BaseStream = baseStream;
        _leaveOpen = leaveOpen;
    }

    /// <summary>
    /// ��ʼ��
    /// </summary>
    public virtual void Init()
    {
        _start = BaseStream.Position;
        BaseStream.Seek(4 + 2 + 4, SeekOrigin.Current);

        _inited = true;
    }

    /// <inheritdoc/>
    public virtual void Write(MixEntry value)
    {
        if (!_inited)
            Init();

        unsafe
        {
            fixed (byte* ptr = _buffer)
            {
                MixEntry* p = &value;
                Buffer.MemoryCopy(p, ptr, sizeof(MixEntry), sizeof(MixEntry));
            }
        }
        BaseStream.Write(_buffer.AsSpan(0, 12));

        _size = Math.Max(_size, value.Offset + value.Size);
        _count++;
    }

    /// <inheritdoc/>
    public virtual async Task WriteAsync(MixEntry value, CancellationToken cancellationToken = default)
    {
        if (!_inited)
            Init();

        unsafe
        {
            fixed (byte* ptr = _buffer)
            {
                MixEntry* p = &value;
                Buffer.MemoryCopy(p, ptr, sizeof(MixEntry), sizeof(MixEntry));
            }
        }
        await BaseStream.WriteAsync(_buffer.AsMemory(0, 12), cancellationToken);

        _size = Math.Max(_size, value.Offset + value.Size);
        _count++;
    }


    /// <summary>
    /// д��Ԫ����
    /// </summary>
    public virtual void WriteMetadata()
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
    internal protected virtual void WriteMetadataDirect(int flag, MixMetadata metadata)
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

    /// <summary>
    /// �ͷ���Դ
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (!_leaveOpen)
                    BaseStream.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// �첽�ͷź���
    /// </summary>
    /// <returns></returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!_leaveOpen)
            await BaseStream.DisposeAsync();
    }

    // ~CsfReader()
    // {
    //     Dispose(disposing: false);
    // }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }
}
