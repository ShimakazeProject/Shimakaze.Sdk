namespace Shimakaze.Sdk.IO.Serialization;

/// <summary>
/// 反序列化器
/// </summary>
/// <typeparam name="T">值的类型</typeparam>
public interface IDeserializer<out T>
{
    /// <summary>
    /// 反序列化
    /// </summary>
    /// <returns>值</returns>
    T Deserialize();
}