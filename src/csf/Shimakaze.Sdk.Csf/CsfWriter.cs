using System.Text;

namespace Shimakaze.Sdk.Csf;

/// <summary>
/// CSF 写入器
/// </summary>
/// <param name="stream"></param>
/// <param name="leaveOpen"></param>
public sealed class CsfWriter(Stream stream, bool leaveOpen = false) : IDisposable
{
    private bool _disposedValue;

    /// <summary>
    /// 写入全部数据
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="csf"></param>
    public static void WriteAllData(Stream stream, CsfData csf)
    {
        using CsfWriter writer = new(stream, leaveOpen: true);
        writer.WriteAllData(csf);
    }
    /// <summary>
    /// 写入元数据
    /// </summary>
    /// <param name="metadata"></param>
    public void WriteMetadata(CsfMetadata metadata)
    {
        stream.Write(metadata.Identifier);
        stream.Write(metadata.Version);
        stream.Write(metadata.LabelCount);
        stream.Write(metadata.StringCount);
        stream.Write(metadata.Unknown);
        stream.Write(metadata.Language);
    }

    /// <summary>
    /// 写入一个标签
    /// </summary>
    /// <param name="label"></param>
    public void WriteLabel(CsfLabel label)
    {
        stream.Write(CsfConstants.LblFlagRaw);
        stream.Write(label.Count);
        stream.Write(Encoding.ASCII.GetByteCount(label.Name));
        stream.Write(Encoding.ASCII.GetBytes(label.Name).AsSpan());

        foreach (var value in label)
            WriteValue(value);
    }

    /// <summary>
    /// 写入一个值
    /// </summary>
    /// <param name="value"></param>
    public void WriteValue(CsfValue value)
    {
        stream.Write(value switch
        {
            { Extra: not null } => CsfConstants.StrwFlgRaw,
            { Extra: null } => CsfConstants.StrFlagRaw,
        });
        stream.Write(Encoding.Unicode.GetByteCount(value.Value) >> 1);
        stream.Write(CsfConstants.CodingValue(Encoding.Unicode.GetBytes(value.Value)));

        if (value.Extra is not null)
        {
            stream.Write(Encoding.ASCII.GetByteCount(value.Extra));
            stream.Write(Encoding.ASCII.GetBytes(value.Extra).AsSpan());
        }
    }

    /// <summary>
    /// 写入全部数据
    /// </summary>
    /// <param name="data"></param>
    public void WriteAllData(CsfData data)
    {
        WriteMetadata(data.Metadata);
        foreach (var label in data)
            WriteLabel(label);
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

    // ~CsfWriter()
    // {
    //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
    //     Dispose(disposing: false);
    // }

    /// <inheritdoc/>
    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        // GC.SuppressFinalize(this);
    }
}
