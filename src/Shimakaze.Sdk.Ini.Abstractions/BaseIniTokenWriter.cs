
namespace Shimakaze.Sdk.Ini;

/// <summary>
/// INI Token 流写入器
/// </summary>
public abstract class BaseIniTokenWriter<TIniDocument, TIniSection>(TextWriter textWriter, bool leaveOpen = false) : IIniTokenWriter<TIniDocument, TIniSection>
    where TIniDocument : IIniDocument<TIniSection>
    where TIniSection : IIniSection
{
    private bool _disposedValue;

    /// <summary>
    /// 基础流
    /// </summary>
    protected TextWriter BaseWriter => textWriter;

    /// <inheritdoc/>
    public abstract void Write(in IIniToken token);

    /// <inheritdoc/>
    public void WriteLine(in IIniToken token)
    {
        Write(token);
        WriteLine();
    }

    /// <inheritdoc/>
    public void WriteLine() => BaseWriter.WriteLine();

    /// <inheritdoc/>
    public void Flush() => BaseWriter.Flush();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Flush();
                if (!leaveOpen)
                    BaseWriter.Dispose();
            }

            _disposedValue = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    ~BaseIniTokenWriter()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public abstract void Write(in TIniDocument document);

    /// <inheritdoc/>
    public abstract void Write(in TIniSection section);

    /// <inheritdoc/>
    public abstract void Write(in KeyValuePair<string, string> keyValuePair);
}