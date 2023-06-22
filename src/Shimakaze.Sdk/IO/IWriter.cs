namespace Shimakaze.Sdk.IO;

/// <summary>
/// 写入器接口
/// </summary>
/// <typeparam name="T">写入的数据的类型</typeparam>
public interface IWriter<T>
{
    /// <summary>
    /// 写入一个值
    /// </summary>
    /// <param name="value">值</param>
    void Write(in T value);
}
