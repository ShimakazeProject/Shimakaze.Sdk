using System.Runtime.InteropServices;
using System.Text;

namespace Shimakaze.Sdk.Csf;

/// <summary>
/// CSF 阅读器
/// </summary>
/// <param name="stream"></param>
/// <param name="leaveOpen"></param>
public sealed class CsfReader(Stream stream, bool leaveOpen = false) : IDisposable
{
    private bool _disposedValue;

    /// <summary>
    /// 从流中读取全部数据
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static CsfData ReadAllData(Stream stream)
    {
        using CsfReader reader = new(stream, leaveOpen: true);
        return reader.ReadAllData();
    }

    /// <summary>
    /// 从流中读取文件头
    /// </summary>
    /// <returns></returns>
    public CsfMetadata ReadMetadata()
    {
        stream.Read<CsfMetadata>(out var head);
        CsfAsserts.IsCsfFile(head.Identifier);
        return head;
    }

    /// <summary>
    /// 从流中读取一个标签
    /// </summary>
    /// <returns></returns>
    public CsfLabel ReadLabel()
    {
        Span<byte> head = stackalloc byte[12];
        var ints = MemoryMarshal.Cast<byte, int>(head);

        stream.ReadExactly(head);
        CsfAsserts.IsLabel(ints[0], () => [stream.Position]);

        int count = ints[1];
        Span<byte> name = stackalloc byte[ints[2]];
        stream.ReadExactly(name);

        CsfLabel label = new(Encoding.ASCII.GetString(name), count);

        for (int i = 0; i < count; i++)
            label.Add(ReadValue());

        return label;
    }

    /// <summary>
    /// 从流中读取全部数据
    /// </summary>
    /// <returns></returns>
    public CsfData ReadAllData()
    {
        CsfData data = new(ReadMetadata());
        for (int i = 0; i < data.Metadata.LabelCount; i++)
            data.Labels.Add(ReadLabel());

        return data;
    }

    /// <summary>
    /// 从流中读取一个值
    /// </summary>
    /// <returns></returns>
    public CsfValue ReadValue()
    {
        Span<byte> head = stackalloc byte[8];
        var ints = MemoryMarshal.Cast<byte, int>(head);

        stream.ReadExactly(head);
        CsfAsserts.IsStringOrExtraString(ints[0], () => [stream.Position]);

        Span<byte> buffer = stackalloc byte[ints[1] << 1];

        stream.ReadExactly(buffer);
        CsfConstants.CodingValue(buffer);
        string value = Encoding.Unicode.GetString(buffer);

        if (ints[0] is not CsfConstants.StrwFlgRaw)
            return new(value, default);

        buffer = stackalloc byte[4];
        stream.ReadExactly(buffer);

        buffer = stackalloc byte[MemoryMarshal.Cast<byte, int>(buffer)[0]];
        stream.ReadExactly(buffer);
        string extra = Encoding.ASCII.GetString(buffer);

        return new(value, extra);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (!leaveOpen)
                    stream.Dispose();
            }

            _disposedValue = true;
        }
    }

    // ~CsfReader()
    // {
    //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //     Dispose(disposing: false);
    // }

    /// <inheritdoc/>
    public void Dispose() =>
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);// GC.SuppressFinalize(this);

}
