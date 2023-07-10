using Shimakaze.Sdk.Ini;

namespace Shimakaze.Sdk.IO.Ini;

/// <summary>
/// Ini反序列化器
/// </summary>
public class IniReader : AsyncReader<IniDocument>
{
    /// <summary>
    /// 基础读取器
    /// </summary>
    public TextReader BaseReader { get; }

    /// <summary>
    /// 构造 Ini反序列化器
    /// </summary>
    /// <param name="stream"> 基础流 </param>
    /// <param name="leaveOpen"> 退出时是否保持流打开 </param>
    public IniReader(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
        BaseReader = new StreamReader(stream, leaveOpen);
    }

    /// <summary>
    /// 反序列化INI
    /// </summary>
    /// <returns> </returns>
    public virtual IniDocument Read()
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

    /// <inheritdoc />
    public override async Task<IniDocument> ReadAsync(IProgress<float>? progress = default, CancellationToken cancellationToken = default)
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

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_leaveOpen)
                BaseReader.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc />
    protected override ValueTask DisposeAsyncCore()
    {
        if (!_leaveOpen)
            BaseReader.Dispose();

        return base.DisposeAsyncCore();
    }
}