namespace Shimakaze.Sdk.Csf;

/// <summary>
/// Csf 写入器
/// </summary>
public interface ICsfWriter : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 写入到流中
    /// </summary>
    /// <param name="value"></param>
    /// <param name="progress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task WriteAsync(CsfDocument value, IProgress<float>? progress = default, CancellationToken cancellationToken = default);
}