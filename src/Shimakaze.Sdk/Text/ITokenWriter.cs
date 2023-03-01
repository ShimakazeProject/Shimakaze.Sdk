using System.Numerics;

namespace Shimakaze.Sdk.Text;

/// <summary>
/// Token Writer.
/// </summary>
/// <typeparam name="TToken">Token Type.</typeparam>
public interface ITokenWriter<in TToken>
    where TToken : struct, INumberBase<TToken>
{
    /// <summary>
    /// Write.
    /// </summary>
    /// <param name="token">Token.</param>
    /// <param name="value">Value.</param>
    void Write(TToken token, string value);
}