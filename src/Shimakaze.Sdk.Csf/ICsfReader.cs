namespace Shimakaze.Sdk.Csf;

/// <summary>
/// Csf 读取器
/// </summary>
public interface ICsfReader
{
    /// <summary>
    /// 从流中读取CSF
    /// </summary>
    /// <param name="progress">进度回报</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CsfDocument> ReadAsync(IProgress<float>? progress = default, CancellationToken cancellationToken = default);
}
