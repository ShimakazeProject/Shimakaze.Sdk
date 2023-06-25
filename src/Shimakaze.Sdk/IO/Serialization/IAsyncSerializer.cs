namespace Shimakaze.Sdk.IO.Serialization;

/// <summary>
/// 异步序列化器
/// </summary>
/// <typeparam name="T">值的类型</typeparam>
/// <typeparam name="TTask">异步方法的返回值类型</typeparam>
public interface IAsyncSerializer<T, TTask>
{
    /// <summary>
    /// 异步序列化
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>可等待的任务</returns>
    TTask SerializeAsync(T value, CancellationToken cancellationToken = default);
}