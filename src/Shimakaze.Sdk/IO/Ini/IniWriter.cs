using Shimakaze.Sdk.Ini;
using Shimakaze.Sdk.Ini.Parser;

namespace Shimakaze.Sdk.IO.Ini;

/// <summary>
/// Ini序列化器
/// </summary>
public sealed class IniWriter : AsyncWriter<IniDocument>
{
    /// <summary>
    /// 基础写入器
    /// </summary>
    public TextWriter BaseWriter { get; }
    private readonly IniTokenWriter _iniTokenWriter;

    /// <summary>
    /// 构造 INI 序列化器
    /// </summary>
    /// <param name="baseStream"> 基础流 </param>
    /// <param name="leaveOpen"> 退出时是否保持流打开 </param>
    public IniWriter(Stream baseStream, bool leaveOpen = false) : base(baseStream, leaveOpen)
    {
        BaseWriter = new StreamWriter(baseStream, leaveOpen: leaveOpen);
        _iniTokenWriter = new(BaseWriter, leaveOpen: leaveOpen);
    }

    /// <summary>
    /// 写入
    /// </summary>
    /// <param name="ini"></param>
    public void Write(in IniDocument ini)
    {
        WriteSectionBody(ini.Default);
        foreach (var section in ini)
        {
            _iniTokenWriter.Write(new(IniTokenType.Section, section.Name));
            _iniTokenWriter.WriteLine();
            WriteSectionBody(section);
        }
    }

    /// <inheritdoc />
    public override Task WriteAsync(IniDocument ini, IProgress<float>? progress = default, CancellationToken cancellationToken = default)
        => Task.Run(() => Write(ini), cancellationToken);

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing"> </param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_leaveOpen)
                _iniTokenWriter.Dispose();
            if (!_leaveOpen)
                BaseWriter.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc />
    protected override ValueTask DisposeAsyncCore()
    {
        if (!_leaveOpen)
            _iniTokenWriter.Dispose();
        if (!_leaveOpen)
            BaseWriter.Dispose();

        return base.DisposeAsyncCore();
    }

    /// <summary>
    /// 写入Section块 不包含Section头
    /// </summary>
    /// <param name="section"> Section </param>
    private void WriteSectionBody(in IniSection section)
    {
        foreach (var item in section)
        {
            _iniTokenWriter.Write(new(IniTokenType.Key, item.Key));
            _iniTokenWriter.Write(new(IniTokenType.Value, item.Value));
            _iniTokenWriter.WriteLine();
        }
    }
}