namespace Shimakaze.Sdk.IO;

/// <summary>
/// 异步读取器接口
/// </summary>
/// <typeparam name="TTask">读出的数据的异步类型</typeparam>
public interface IAsyncReader<TTask>
{
    /// <summary>
    /// 异步的读取一个值
    /// </summary>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>异步的值</returns>
    TTask ReadAsync(CancellationToken cancellationToken = default);
}
