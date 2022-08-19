namespace Shimakaze.Sdk.Preprocessor.Ini;

/// <summary>
/// 预处理器命令接口
/// </summary>
public interface IPreprocessorCommand
{
    /// <summary>
    /// 预处理器命令名
    /// </summary>
    string Command { get; }
    /// <summary>
    /// 初始化过程
    /// </summary>
    /// <param name="preprocessor">预处理器实例</param>
    Task InitializeAsync(IniPreprocessor preprocessor) => Task.CompletedTask;

    /// <summary>
    /// 预处理器命令执行
    /// </summary>
    /// <param name="args">命令参数</param>
    /// <param name="preprocessor">预处理器实例</param>
    /// <returns></returns>
    Task ExecuteAsync(string[] args, IniPreprocessor preprocessor);

    /// <summary>
    /// 需要后处理
    /// </summary>
    bool IsPostProcessing => false;

    /// <summary>
    /// 后处理
    /// </summary>
    /// <param name="preprocessor">预处理器实例</param>
    Task PostProcessingAsync(IniPreprocessor preprocessor) => Task.CompletedTask;
}
