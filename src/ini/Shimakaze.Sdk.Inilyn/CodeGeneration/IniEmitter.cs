using System.Text;

namespace Shimakaze.Sdk.Inilyn.CodeGeneration;

/// <summary>
/// INI 输出格式化器。
/// </summary>
/// <remarks>
/// 负责将结构化的节和键值对写入紧凑的标准 INI 格式。
/// 输出无注释、无多余空白，键值对为 <c>key=value</c> 紧凑格式。
/// </remarks>
public sealed class IniEmitter
{
    private readonly StringBuilder _buffer = new();

    /// <summary>
    /// 写入一个节声明头。
    /// </summary>
    /// <param name="sectionName">节名。</param>
    public void WriteSectionHeader(string sectionName)
    {
        _buffer.Append('[');
        _buffer.Append(sectionName);
        _buffer.Append(']');
        _buffer.Append('\n');
    }

    /// <summary>
    /// 写入一个键值对。
    /// </summary>
    /// <param name="key">键名。</param>
    /// <param name="value">值。</param>
    public void WriteKeyValue(string key, string value)
    {
        _buffer.Append(key);
        _buffer.Append('=');
        _buffer.Append(value);
        _buffer.Append('\n');
    }

    /// <summary>
    /// 获取生成的结果文本。
    /// </summary>
    /// <returns>生成的 INI 文本。</returns>
    public override string ToString() => _buffer.ToString();
}
