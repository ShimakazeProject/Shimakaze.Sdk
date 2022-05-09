namespace Shimakaze.Sdk.Preprocessor;

/// <summary>
/// 此异常表示变量不存在
/// </summary>
[Serializable]
public class VariableNotInitializeException : Exception
{
    /// <summary>
    /// 空构造函数
    /// </summary>
    public VariableNotInitializeException() { }
    /// <summary>
    /// 构造函数
    /// </summary>
    public VariableNotInitializeException(string message) : base(message) { }
    /// <summary>
    /// 构造函数
    /// </summary>
    public VariableNotInitializeException(string message, Exception inner) : base(message, inner) { }
    /// <summary>
    /// 构造函数
    /// </summary>
    protected VariableNotInitializeException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}