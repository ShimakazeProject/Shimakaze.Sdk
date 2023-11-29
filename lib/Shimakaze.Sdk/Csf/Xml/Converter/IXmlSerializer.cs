using System.Xml;

namespace Shimakaze.Sdk.Csf.Xml.Converter;

/// <summary>
/// XML序列化器
/// </summary>
/// <typeparam name="T"> 类型 </typeparam>
public interface IXmlSerializer<T>
{
    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="reader"> XML读取器 </param>
    /// <returns> 对象 </returns>
    T Deserialize(XmlReader reader);

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="writer"> XML写入器 </param>
    /// <param name="value"> 对象 </param>
    void Serialize(XmlWriter writer, T value);
}