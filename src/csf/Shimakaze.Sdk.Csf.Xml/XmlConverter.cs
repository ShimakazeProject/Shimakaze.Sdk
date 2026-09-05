using System.Xml;

namespace Shimakaze.Sdk.Csf.Xml;

/// <summary>
/// XML转换器
/// </summary>
public abstract class XmlConverter
{
    /// <summary>
    /// 获取转换器支持的类型
    /// </summary>
    public abstract Type? Type { get; }

    /// <summary>
    /// 确定指定的类型是否可以被转换器转换
    /// </summary>
    /// <param name="typeToConvert"></param>
    /// <returns></returns>
    public abstract bool CanConvert(Type typeToConvert);
}

/// <summary>
/// XML转换器
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class XmlConverter<T> : XmlConverter
{
    /// <inheritdoc/>
    public sealed override Type Type { get; } = typeof(T);

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(T);

    /// <summary>
    /// 读取 XML 并将其转换为对象
    /// </summary>
    /// <param name="reader">XML读取器</param>
    /// <param name="options">XML序列化选项</param>
    /// <returns>对象</returns>
    public abstract T? Read(XmlReader reader, XmlSerializerOptions options);

    /// <summary>
    /// 将对象写入 XML
    /// </summary>
    /// <param name="writer">XML写入器</param>
    /// <param name="value">对象</param>
    /// <param name="options">XML序列化选项</param>
    public abstract void Write(XmlWriter writer, T value, XmlSerializerOptions options);
}
