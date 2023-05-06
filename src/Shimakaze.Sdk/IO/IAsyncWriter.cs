namespace Shimakaze.Sdk.IO;

/// <summary>
/// 异步写入器接口
/// </summary>
/// <typeparam name="T">写入的数据的类型</typeparam>
/// <typeparam name="TTask">异步方法的返回类型</typeparam>
public interface IAsyncWriter<T, TTask>
{
    /// <summary>
    /// 异步的写入一个值
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>可等待的任务</returns>
    TTask WriteAsync(T value, CancellationToken cancellationToken = default);
}