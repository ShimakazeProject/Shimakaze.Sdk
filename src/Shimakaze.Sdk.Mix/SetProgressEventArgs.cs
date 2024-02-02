namespace Shimakaze.Sdk.Mix;

/// <summary>
/// 进度回报事件参数
/// </summary>
public sealed class SetProgressEventArgs : EventArgs
{
    /// <summary>
    /// 进度回报
    /// </summary>
    public IProgress<int>? Progress { get; set; }
}