namespace Shimakaze.Sdk.Preprocessor.Ini;

/// <summary>
/// Ԥ����������ӿ�
/// </summary>
public interface IPreprocessorCommand
{
    /// <summary>
    /// Ԥ������������
    /// </summary>
    string Command { get; }
    /// <summary>
    /// ��ʼ������
    /// </summary>
    /// <param name="preprocessor">Ԥ������ʵ��</param>
    Task InitializeAsync(IniPreprocessor preprocessor) => Task.CompletedTask;

    /// <summary>
    /// Ԥ����������ִ��
    /// </summary>
    /// <param name="args">�������</param>
    /// <param name="preprocessor">Ԥ������ʵ��</param>
    /// <returns></returns>
    Task ExecuteAsync(string[] args, IniPreprocessor preprocessor);

    /// <summary>
    /// ��Ҫ����
    /// </summary>
    bool IsPostProcessing => false;

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="preprocessor">Ԥ������ʵ��</param>
    Task PostProcessingAsync(IniPreprocessor preprocessor) => Task.CompletedTask;
}
