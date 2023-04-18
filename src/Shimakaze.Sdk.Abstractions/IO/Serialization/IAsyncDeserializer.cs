namespace Shimakaze.Sdk.IO.Serialization;

/// <summary>
/// 异步反序列化器
/// </summary>
/// <typeparam name="TTask">异步方法的返回值类型</typeparam>
public interface IAsyncDeserializer<TTask>
{
    /// <summary>
    /// 异步反序列化
    /// </summary>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>包含返回值的可等待的任务</returns>
    TTask DeserializeAsync(CancellationToken cancellationToken = default);
}