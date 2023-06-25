namespace Shimakaze.Sdk.IO;

/// <summary>
/// 读取器接口
/// </summary>
/// <typeparam name="T">读出的数据的类型</typeparam>
public interface IReader<out T>
{
    /// <summary>
    /// 读取一个值
    /// </summary>
    /// <returns>值</returns>
    T Read();
}