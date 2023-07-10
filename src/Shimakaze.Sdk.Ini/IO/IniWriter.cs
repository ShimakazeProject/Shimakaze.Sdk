using Shimakaze.Sdk.Ini;

namespace Shimakaze.Sdk.IO.Ini;

/// <summary>
/// Ini序列化器
/// </summary>
public class IniWriter : AsyncWriter<IniDocument>
{
    /// <summary>
    /// 基础写入器
    /// </summary>
    public TextWriter BaseWriter { get; }

    /// <summary>
    /// 构造 INI 序列化器
    /// </summary>
    /// <param name="stream"> 基础流 </param>
    /// <param name="leaveOpen"> 退出时是否保持流打开 </param>
    public IniWriter(Stream stream, bool leaveOpen = false) : base(stream, leaveOpen)
    {
        BaseWriter = new StreamWriter(stream, leaveOpen: leaveOpen);
    }

    /// <inheritdoc />
    public override async Task WriteAsync(IniDocument value, IProgress<float>? progress = default, CancellationToken cancellationToken = default)
    {
        await WriteSectionBodyAsync(value.Default, cancellationToken);
        var map = value.Values.ToArray();
        for (int i = 0; i < map.Length; i++)
        {
            var item = map[i];
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((float)i / map.Length);
            await BaseWriter.WriteLineAsync($"[{item.Name}]");
            await WriteSectionBodyAsync(item, cancellationToken);
        }
    }

    /// <summary>
    /// 序列化Ini文档
    /// </summary>
    /// <param name="value"> value </param>
    public virtual void Writer(in IniDocument value)
    {
        WriteSectionBody(value.Default);
        foreach (var item in value)
        {
            BaseWriter.WriteLine($"[{item.Name}]");
            WriteSectionBody(item);
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="disposing"> </param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_leaveOpen)
                BaseWriter.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc />
    protected override ValueTask DisposeAsyncCore()
    {
        if (!_leaveOpen)
            BaseWriter.Dispose();

        return base.DisposeAsyncCore();
    }

    /// <summary>
    /// 写入Section块 不包含Section头
    /// </summary>
    /// <param name="section"> Section </param>
    protected virtual void WriteSectionBody(IniSection section)
    {
        foreach (var item in section)
            BaseWriter.WriteLine($"{item.Key}={item.Value}");
    }

    /// <inheritdoc cref="WriteSectionBody" />
    /// <inheritdoc cref="WriteAsync" />
    protected virtual async Task WriteSectionBodyAsync(IniSection section, CancellationToken cancellationToken)
    {
        foreach (var item in section)
            await BaseWriter.WriteLineAsync($"{item.Key}={item.Value}");
    }
}