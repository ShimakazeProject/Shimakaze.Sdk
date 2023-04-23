using Shimakaze.Sdk.Ini;
using Shimakaze.Sdk.IO.Serialization;

namespace Shimakaze.Sdk.IO.Ini.Serialization;

/// <summary>
/// Ini反序列化器
/// </summary>
public class IniDeserializer : IDeserializer<IniDocument>, IAsyncDeserializer<Task<IniDocument>>, IDisposable
{
    private bool _disposedValue;
    private readonly bool _leaveOpen;

    /// <summary>
    /// 基础读取器
    /// </summary>
    public TextReader BaseReader { get; }

    /// <summary>
    /// 构造 Ini反序列化器
    /// </summary>
    /// <param name="reader">基础流</param>
    /// <param name="leaveOpen">退出时是否保持流打开</param>
    public IniDeserializer(TextReader reader, bool leaveOpen = false)
    {
        BaseReader = reader;
        _leaveOpen = leaveOpen;
    }

    /// <summary>
    /// 反序列化INI
    /// </summary>
    /// <returns></returns>
    public virtual IniDocument Deserialize()
    {
        IniDocument doc = new();
        IniSection current = doc.Default;
        string? line;
        while ((line = BaseReader.ReadLine()) is not null)
        {
            line = line.Split(';', '#').First().Trim();

            if (string.IsNullOrEmpty(line))
                continue;

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                current = new()
                {
                    Name = line.Substring(1, line.Length - 2)
                };
                doc.Add(current);

                continue;
            }

            var index = line.IndexOf('=');
            if (index is -1)
                current.Add(line, string.Empty);
            else
                current.Add(line.Substring(0, index).TrimEnd(), line.Substring(index + 1).TrimStart());
        }
        return doc;
    }

    /// <inheritdoc cref="Deserialize"/>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>异步任务</returns>
    public virtual async Task<IniDocument> DeserializeAsync(CancellationToken cancellationToken = default)
    {
        IniDocument doc = new();
        IniSection current = doc.Default;
        string? line;
        while ((line = await BaseReader.ReadLineAsync(cancellationToken)) is not null)
        {
            line = line.Split(';', '#').First().Trim();

            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(line))
                continue;

            cancellationToken.ThrowIfCancellationRequested();
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                current = new()
                {
                    Name = line.Substring(1, line.Length - 2)
                };
                doc.Add(current);

                continue;
            }

            cancellationToken.ThrowIfCancellationRequested();
            var index = line.IndexOf('=');
            if (index is -1)
                current.Add(line, string.Empty);
            else
                current.Add(line.Substring(0, index).TrimEnd(), line.Substring(index + 1).TrimStart());
        }
        return doc;
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
                    BaseReader.Dispose();
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