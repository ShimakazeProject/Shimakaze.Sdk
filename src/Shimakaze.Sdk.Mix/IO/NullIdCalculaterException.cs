using System.Runtime.CompilerServices;

namespace Shimakaze.Sdk.IO.Mix;

/// <summary>
/// IdCalculater没有设置
/// </summary>
[Serializable, CompilerGenerated]
public class NullIdCalculaterException : Exception
{
    /// <inheritdoc/>
    public NullIdCalculaterException() { }
    /// <inheritdoc/>
    public NullIdCalculaterException(string message) : base(message) { }
    /// <inheritdoc/>
    public NullIdCalculaterException(string message, Exception inner) : base(message, inner) { }
    /// <inheritdoc/>
    protected NullIdCalculaterException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}