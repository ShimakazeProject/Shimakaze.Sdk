using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Preprocessor.Kernel;

/// <summary>
/// 预处理器异常
/// </summary>
[Serializable]

public class PreprocessorException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    public PreprocessorException() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public PreprocessorException(string message) : base(message) { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public PreprocessorException(string message, Exception inner) : base(message, inner) { }
}