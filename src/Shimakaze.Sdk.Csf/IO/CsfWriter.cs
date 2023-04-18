using System.Runtime.InteropServices;
using System.Text;

using Shimakaze.Sdk.Csf;

namespace Shimakaze.Sdk.IO.Csf;

/// <summary>
/// Csf 写入器
/// </summary>
public class CsfWriter : IWriter<CsfData>, IAsyncWriter<CsfData, Task>, IDisposable, IAsyncDisposable
{
    private bool _disposedValue;
    private readonly bool _leaveOpen;

    /// <summary>
    /// 是否已经初始化过
    /// </summary>
    protected bool _inited;
    /// <summary>
    /// 当前Data标签的个数
    /// </summary>
    protected int _labelCount;
    /// <summary>
    /// 当前Value标签的个数
    /// </summary>
    protected int _valueCount;
    /// <summary>
    /// 流开始的位置
    /// </summary>
    protected long _start;

    /// <summary>
    /// 基础流
    /// </summary>
    public Stream BaseStream { get; }

    /// <summary>
    /// 构造 Csf 写入器
    /// </summary>
    /// <param name="baseStream">基础流</param>
    /// <param name="leaveOpen">退出时是否保持流打开</param>
    /// <exception cref="NotSupportedException">当流不支持Seek时抛出</exception>
    public CsfWriter(Stream baseStream, bool leaveOpen = false)
    {
        if (!baseStream.CanSeek)
            throw new NotSupportedException("The Stream cannot support Seek.");

        BaseStream = baseStream;
        _leaveOpen = leaveOpen;
    }
    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init()
    {
        _start = BaseStream.Position;
        BaseStream.Seek(Marshal.SizeOf<CsfMetadata>(), SeekOrigin.Current);

        _inited = true;
    }

    /// <summary>
    /// 写入一个值
    /// </summary>
    /// <param name="value">值</param>
    protected virtual void WriteValue(CsfValue value)
    {
        try
        {
            BaseStream.Write(BitConverter.GetBytes(value.Identifier));
            BaseStream.Write(BitConverter.GetBytes(value.ValueLength));
            BaseStream.Write(CsfConstants.CodingValue(Encoding.Unicode.GetBytes(value.Value)));
            if (value is not CsfValueExtra extra)
                return;

            BaseStream.Write(BitConverter.GetBytes(extra.ExtraValueLength));
            BaseStream.Write(Encoding.ASCII.GetBytes(extra.ExtraValue));
        }
        finally
        {
            _valueCount++;
        }
    }

    /// <inheritdoc cref="WriteValue"/>
    /// <inheritdoc cref="CsfReader.InitAsync"/>
    protected virtual async Task WriteValueAsync(CsfValue value, CancellationToken cancellationToken = default)
    {
        try
        {
            await BaseStream.WriteAsync(BitConverter.GetBytes(value.Identifier), cancellationToken);
            await BaseStream.WriteAsync(BitConverter.GetBytes(value.ValueLength), cancellationToken);
            await BaseStream.WriteAsync(CsfConstants.CodingValue(Encoding.Unicode.GetBytes(value.Value)), cancellationToken);
            if (value is not CsfValueExtra extra)
                return;

            await BaseStream.WriteAsync(BitConverter.GetBytes(extra.ExtraValueLength), cancellationToken);
            await BaseStream.WriteAsync(Encoding.ASCII.GetBytes(extra.ExtraValue), cancellationToken);
        }
        finally
        {
            _valueCount++;
        }
    }

    /// <inheritdoc/>
    public virtual void Write(CsfData value)
    {
        if (!_inited)
            Init();

        try
        {
            BaseStream.Write(BitConverter.GetBytes(value.Identifier));
            BaseStream.Write(BitConverter.GetBytes(value.StringCount));
            BaseStream.Write(BitConverter.GetBytes(value.LabelNameLength));
            BaseStream.Write(Encoding.ASCII.GetBytes(value.LabelName));
            if (!value.Values.Any())
            {
                BaseStream.Write(BitConverter.GetBytes(CsfConstants.StrFlagRaw));
                BaseStream.Write(BitConverter.GetBytes(0));
                return;
            }

            foreach (CsfValue i in value.Values)
                WriteValue(i);
        }
        finally
        {
            _labelCount++;
        }

    }

    /// <inheritdoc/>
    public virtual async Task WriteAsync(CsfData value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_inited)
            Init();

        try
        {
            await BaseStream.WriteAsync(BitConverter.GetBytes(value.Identifier), cancellationToken);
            await BaseStream.WriteAsync(BitConverter.GetBytes(value.StringCount), cancellationToken);
            await BaseStream.WriteAsync(BitConverter.GetBytes(value.LabelNameLength), cancellationToken);
            await BaseStream.WriteAsync(Encoding.ASCII.GetBytes(value.LabelName), cancellationToken);
            if (!value.Values.Any())
            {
                await BaseStream.WriteAsync(BitConverter.GetBytes(CsfConstants.StrFlagRaw), cancellationToken);
                await BaseStream.WriteAsync(BitConverter.GetBytes(0), cancellationToken);
                return;
            }

            foreach (CsfValue i in value.Values)
                await WriteValueAsync(i, cancellationToken);
        }
        finally
        {
            _labelCount++;
        }
    }

    /// <summary>
    /// 写入元数据
    /// </summary>
    /// <param name="version">版本 2 或 3</param>
    /// <param name="language">语言</param>
    /// <param name="unknown">未知</param>
    public virtual void WriteMetadata(int version = 3, int language = 0, int unknown = 0)
    {
        long current = BaseStream.Position;
        BaseStream.Seek(_start, SeekOrigin.Begin);
        WriteMetadataDirect(new(version, language)
        {
            LabelCount = _labelCount,
            StringCount = _valueCount,
            Unknown = unknown
        });
        BaseStream.Seek(current, SeekOrigin.Begin);
    }

    /// <summary>
    /// 直接写入元数据
    /// </summary>
    /// <param name="metadata">元数据</param>
    internal protected virtual void WriteMetadataDirect(CsfMetadata metadata)
    {
        unsafe
        {
            nint ptr = Marshal.AllocHGlobal(sizeof(CsfMetadata));
            try
            {
                Marshal.StructureToPtr(metadata, ptr, true);
                BaseStream.Write(new Span<byte>((byte*)ptr, sizeof(CsfMetadata)));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }


    /// <summary>
    /// 释放资源
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
    /// 异步释放核心
    /// </summary>
    /// <returns></returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!_leaveOpen)
            await BaseStream.DisposeAsync();
    }

    // ~CsfWriter()
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
