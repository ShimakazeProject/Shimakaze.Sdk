using Shimakaze.Sdk.Ini;
using Shimakaze.Sdk.IO.Serialization;

namespace Shimakaze.Sdk.IO.Ini.Serialization;

/// <summary>
/// Ini序列化器
/// </summary>
public class IniSerializer : ISerializer<IniDocument>, IAsyncSerializer<IniDocument, Task>, IDisposable
{
    private bool _disposedValue;
    private readonly bool _leaveOpen;

    /// <summary>
    /// 基础写入器
    /// </summary>
    public TextWriter BaseWriter { get; }

    /// <summary>
    /// 构造 INI 序列化器
    /// </summary>
    /// <param name="writer">基础流</param>
    /// <param name="leaveOpen">退出时是否保持流打开</param>
    public IniSerializer(TextWriter writer, bool leaveOpen = false)
    {
        BaseWriter = writer;
        _leaveOpen = leaveOpen;
    }

    /// <summary>
    /// 序列化Ini文档
    /// </summary>
    /// <param name="value">value</param>
    public virtual void Serialize(IniDocument value)
    {
        WriteSectionBody(value.Default);
        foreach (var item in value)
        {
            BaseWriter.WriteLine($"[{item.Name}]");
            WriteSectionBody(item);
        }
    }


    /// <inheritdoc cref="Serialize"/>
    /// <param name="value">value</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>异步任务</returns>
    public virtual async Task SerializeAsync(IniDocument value, CancellationToken cancellationToken = default)
    {
        await WriteSectionBodyAsync(value.Default, cancellationToken);
        foreach (var item in value)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await BaseWriter.WriteLineAsync($"[{item.Name}]");
            await WriteSectionBodyAsync(item, cancellationToken);
        }
    }

    /// <summary>
    /// 写入Section块 不包含Section头
    /// </summary>
    /// <param name="section">Section</param>
    protected virtual void WriteSectionBody(IniSection section)
    {
        foreach (var item in section)
            BaseWriter.WriteLine($"{item.Key}=${item.Value}");
    }

    /// <inheritdoc cref="WriteSectionBody"/>
    /// <inheritdoc cref="SerializeAsync"/>
    protected virtual async Task WriteSectionBodyAsync(IniSection section, CancellationToken cancellationToken)
    {
        foreach (var item in section)
            await BaseWriter.WriteLineAsync($"{item.Key}=${item.Value}");
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
                    BaseWriter.Dispose();
            }

            _disposedValue = true;
        }
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

}