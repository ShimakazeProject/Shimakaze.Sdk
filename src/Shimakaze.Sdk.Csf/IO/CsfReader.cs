using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

using Shimakaze.Sdk.Csf;


namespace Shimakaze.Sdk.IO.Csf;

/// <summary>
/// Csf ��ȡ��
/// </summary>
public class CsfReader : IReader<CsfData>, IAsyncReader<Task<CsfData>>, IDisposable, IAsyncDisposable
{
    private bool _disposedValue;
    private readonly bool _leaveOpen;
    private readonly byte[] _buffer;

    /// <summary>
    /// Label��ǩ����
    /// </summary>
    protected const int SizeOfLabel = sizeof(int) * 3;
    /// <summary>
    /// Value��ǩ����
    /// </summary>
    protected const int SizeOfValue = sizeof(int) * 2;
    /// <summary>
    /// �Ƿ��Ѿ���ʼ����
    /// </summary>
    protected bool _inited;
    /// <summary>
    /// ��ǰ��ǩ�ĸ���
    /// </summary>
    protected int _current;

    /// <summary>
    /// ������
    /// </summary>
    public Stream BaseStream { get; }
    /// <summary>
    /// Csf Ԫ����
    /// </summary>
    public CsfMetadata Metadata { get; protected set; }

    /// <summary>
    /// ���� Csf ��ȡ��
    /// </summary>
    /// <param name="baseStream">������</param>
    /// <param name="buffer">��������С</param>
    /// <param name="leaveOpen">�˳�ʱ�Ƿ񱣳�����</param>
    public CsfReader(Stream baseStream, bool leaveOpen = false, byte[]? buffer = default)
    {
        _buffer = buffer ?? new byte[1024];
        if (_buffer.Length % 2 is not 0)
            throw new NotSupportedException("The buffer length MUST be divisible by 2!");
        BaseStream = baseStream;
        _leaveOpen = leaveOpen;
    }

    /// <summary>
    /// ��ʼ��
    /// </summary>
    public virtual void Init()
    {
        unsafe
        {
            int size = BaseStream.Read(_buffer.AsSpan(0, sizeof(CsfMetadata)));
            fixed (byte* ptr = _buffer)
            {
                Metadata = Marshal.PtrToStructure<CsfMetadata>((nint)ptr);
                CsfThrowHelper.IsCsfFile(*(int*)ptr);
            }
        }

        _inited = true;
    }

    /// <inheritdoc cref="Init"/>
    /// <param name="cancellationToken">ȡ��</param>
    /// <returns>�ɵȴ�ֵ</returns>
    public virtual async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        int size = await BaseStream.ReadAsync(_buffer.AsMemory(0, Marshal.SizeOf<CsfMetadata>()), cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        unsafe
        {
            fixed (byte* ptr = _buffer)
            {
                Metadata = Marshal.PtrToStructure<CsfMetadata>((nint)ptr);
                Debug.WriteLine(Metadata);
                CsfThrowHelper.IsCsfFile(*(int*)ptr);
            }
        }

        _inited = true;
    }

    /// <summary>
    /// ��ȡһ��Value
    /// </summary>
    /// <param name="i">��ǰValue�����</param>
    /// <returns>ֵ</returns>
    protected virtual CsfValue ReadValue(int i)
    {
        int flag, length;
        StringBuilder value = new();
        unsafe
        {
            int size = BaseStream.Read(_buffer.AsSpan(0, SizeOfValue));
            fixed (byte* ptr = _buffer)
            {
                int* p = (int*)ptr;
                flag = *p++;
                length = *p++;
                Debug.WriteLine($"  Values Info:", "value");
                Debug.WriteLine($"    Offset: 0x{BaseStream.Position - SizeOfValue:X8}", "value");
                Debug.WriteLine($"    Flag: 0x{flag:X8}", "value");
                Debug.WriteLine($"    String Length: {length}", "value");
                CsfThrowHelper.IsStringOrExtraString(flag, () => new object[] { _current, i, BaseStream.Position - SizeOfValue });
            }

            if (length <= 0)
            {
                Debug.WriteLine($"    This value haven't any characters!", "value");
                return new();
            }

            int readCount = length << 1;
            while (readCount > 0)
            {
                size = BaseStream.Read(_buffer.AsSpan(0, Math.Min(_buffer.Length, readCount)));
                readCount -= size;
                fixed (byte* ptr = _buffer)
                {
                    CsfConstants.CodingValue(ptr, size);
                    value.Append(Encoding.Unicode.GetString(ptr, size));
                }
            }
            Debug.WriteLine($"    Value: {value}", "value");

            if (flag is not CsfConstants.StrwFlgRaw)
                return new CsfValue(flag, length, value.ToString());

            size = BaseStream.Read(_buffer.AsSpan(0, sizeof(int)));
            int extralength = BitConverter.ToInt32(_buffer, 0);
            Debug.WriteLine($"    Extra:", "extra");
            Debug.WriteLine($"      Extra Length: {extralength}", "extra");
            size = BaseStream.Read(_buffer.AsSpan(0, extralength));
            string extra = Encoding.ASCII.GetString(_buffer, 0, extralength);
            Debug.WriteLine($"      Extra: {extra}", "extra");
            return new CsfValueExtra(flag, length, value.ToString(), extralength, extra);
        }
    }

    /// <inheritdoc cref="ReadValue"/>
    /// <inheritdoc cref="InitAsync"/>
    protected virtual async Task<CsfValue> ReadValueAsync(int i, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        int flag, length;
        StringBuilder value = new();

        int size = await BaseStream.ReadAsync(_buffer.AsMemory(0, SizeOfValue), cancellationToken);
        unsafe
        {
            fixed (byte* ptr = _buffer)
            {
                int* p = (int*)ptr;
                flag = *p++;
                length = *p++;
                Debug.WriteLine($"  Values Info:", "value");
                Debug.WriteLine($"    Offset: 0x{BaseStream.Position - SizeOfValue:X8}", "value");
                Debug.WriteLine($"    Flag: 0x{flag:X8}", "value");
                Debug.WriteLine($"    String Length: {length}", "value");
                CsfThrowHelper.IsStringOrExtraString(flag, () => new object[] { _current, i, BaseStream.Position - SizeOfValue });
            }
        }

        if (length <= 0)
        {
            Debug.WriteLine($"    This value haven't any characters!", "value");
            return new();
        }

        cancellationToken.ThrowIfCancellationRequested();

        int readCount = length << 1;
        while (readCount > 0)
        {
            size = await BaseStream.ReadAsync(_buffer.AsMemory(0, Math.Min(_buffer.Length, readCount)), cancellationToken);
            readCount -= size;
            unsafe
            {
                fixed (byte* ptr = _buffer)
                {
                    CsfConstants.CodingValue(ptr, size);
                    value.Append(Encoding.Unicode.GetString(ptr, size));
                }
            }
        }
        Debug.WriteLine($"    Value: {value}", "value");

        if (flag is not CsfConstants.StrwFlgRaw)
            return new CsfValue(flag, length, value.ToString());

        cancellationToken.ThrowIfCancellationRequested();
        size = await BaseStream.ReadAsync(_buffer.AsMemory(0, sizeof(int)), cancellationToken);
        int extralength = BitConverter.ToInt32(_buffer, 0);
        Debug.WriteLine($"    Extra:", "extra");
        Debug.WriteLine($"      Extra Length: {extralength}", "extra");
        size = await BaseStream.ReadAsync(_buffer.AsMemory(0, extralength), cancellationToken);
        string extra = Encoding.ASCII.GetString(_buffer, 0, extralength);
        Debug.WriteLine($"      Extra: {extra}", "extra");
        return new CsfValueExtra(flag, length, value.ToString(), extralength, extra);

    }

    /// <summary>
    /// ��ȡһ��Data
    /// </summary>
    /// <returns>ֵ</returns>
    public virtual CsfData Read()
    {
        if (!_inited)
            Init();

        int flag, count, length;
        string label;
        CsfData result;

        try
        {
            unsafe
            {
                int size = BaseStream.Read(_buffer.AsSpan(0, SizeOfLabel));
                fixed (byte* ptr = _buffer)
                {
                    int* p = (int*)ptr;
                    flag = *p++;
                    count = *p++;
                    length = *p++;
                    Debug.WriteLine($"#{_current:D6}", "label");
                    Debug.WriteLine($"  Offset: 0x{BaseStream.Position - SizeOfLabel:X8}", "label");
                    Debug.WriteLine($"  Flag: 0x{flag:X8}", "label");
                    Debug.WriteLine($"  String Count: {count}", "label");
                    Debug.WriteLine($"  Label Name Length: {length}", "label");
                    CsfThrowHelper.IsLabel(flag, () => new object[] { _current, BaseStream.Position - SizeOfLabel });
                }
                size = BaseStream.Read(_buffer.AsSpan(0, length));
                fixed (byte* ptr = _buffer)
                {
                    label = Encoding.ASCII.GetString(ptr, length);
                    Debug.WriteLine($"  Label Name: {label}", "label");
                }

                result = new(flag, count, length, label);
            }

            for (int i = 0; i < count; i++)
                result.Add(ReadValue(i));

            return result;
        }
        finally
        {
            _current++;
        }
    }

    /// <inheritdoc cref="Read"/>
    /// <inheritdoc cref="InitAsync"/>
    public virtual async Task<CsfData> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (!_inited)
            await InitAsync(cancellationToken);

        int flag, count, length;
        string label;
        CsfData result;

        try
        {
            int size = await BaseStream.ReadAsync(_buffer.AsMemory(0, SizeOfLabel), cancellationToken);
            unsafe
            {
                fixed (byte* ptr = _buffer)
                {
                    int* p = (int*)ptr;
                    flag = *p++;
                    count = *p++;
                    length = *p++;
                    Debug.WriteLine($"#{_current:D6}", "label");
                    Debug.WriteLine($"  Offset: 0x{BaseStream.Position - SizeOfLabel:X8}", "label");
                    Debug.WriteLine($"  Flag: 0x{flag:X8}", "label");
                    Debug.WriteLine($"  String Count: {count}", "label");
                    Debug.WriteLine($"  Label Name Length: {length}", "label");
                    CsfThrowHelper.IsLabel(flag, () => new object[] { _current, BaseStream.Position - SizeOfLabel });
                }
            }
            size = await BaseStream.ReadAsync(_buffer.AsMemory(0, length), cancellationToken);
            unsafe
            {
                fixed (byte* ptr = _buffer)
                {
                    label = Encoding.ASCII.GetString(ptr, length);
                    Debug.WriteLine($"  Label Name: {label}", "label");
                }
            }

            result = new(flag, count, length, label);


            for (int i = 0; i < count; i++)
                result.Add(await ReadValueAsync(i, cancellationToken));

            return result;
        }
        catch
        {
            _current--;
            throw;
        }
        finally
        {
            _current++;
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