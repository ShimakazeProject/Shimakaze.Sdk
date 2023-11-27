using Shimakaze.Sdk.Ini;
using Shimakaze.Sdk.Ini.Binder;
using Shimakaze.Sdk.Ini.Parser;

namespace Shimakaze.Sdk.IO.Ini;

/// <summary>
/// Ini反序列化器
/// </summary>
public sealed class IniReader : AsyncReader<IniDocument>
{
    /// <summary>
    /// 基础读取器
    /// </summary>
    private TextReader BaseReader { get; }
    private readonly IniTokenReader _tokenReader;
    private readonly IniDocumentBinder _binder;

    /// <summary>
    /// 构造 Ini反序列化器
    /// </summary>
    /// <param name="stream"> 基础流 </param>
    /// <param name="leaveOpen"> 退出时是否保持流打开 </param>
    public IniReader(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
        BaseReader = new StreamReader(stream, leaveOpen: leaveOpen);
        _tokenReader = new(BaseReader, leaveOpen: leaveOpen);
        _binder = new(_tokenReader, leaveOpen: leaveOpen);
    }

    /// <summary>
    /// 读取一个 INI 文档
    /// </summary>
    /// <returns></returns>
    public IniDocument Read()
    {
        IniDocument ini = [];
        _binder.Bind(ini);
        return ini;
    }

    /// <inheritdoc />
    public override Task<IniDocument> ReadAsync(IProgress<float>? progress = default, CancellationToken cancellationToken = default) => Task.Run(Read, cancellationToken);

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_leaveOpen)
                _binder.Dispose();
            if (!_leaveOpen)
                _tokenReader.Dispose();
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