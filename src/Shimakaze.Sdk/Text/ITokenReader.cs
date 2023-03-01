using System.Numerics;

namespace Shimakaze.Sdk.Text;

/// <summary>
/// Token Reader.
/// </summary>
/// <typeparam name="TToken">Token Type.</typeparam>
public interface ITokenReader<TToken>
    where TToken : struct, INumberBase<TToken>
{
    /// <summary>
    /// Gets current value.
    /// </summary>
    string Value { get; }

    /// <summary>
    /// Gets current token.
    /// </summary>
    TToken? Token { get; }

    /// <summary>
    /// Read Next.
    /// </summary>
    /// <returns>Success.</returns>
    bool Read();
}