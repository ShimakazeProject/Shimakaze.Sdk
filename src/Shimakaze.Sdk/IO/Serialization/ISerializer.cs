namespace Shimakaze.Sdk.IO.Serialization;

/// <summary>
/// 序列化器
/// </summary>
/// <typeparam name="T">值的类型</typeparam>
public interface ISerializer<T>
{
    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="value">值</param>
    void Serialize(in T value);
}