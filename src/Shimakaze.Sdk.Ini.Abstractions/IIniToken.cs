namespace Shimakaze.Sdk.Ini;

/// <summary>
/// Ini Token
/// </summary>
public interface IIniToken
{
    /// <summary>
    /// Token
    /// </summary>
    public int Token { get; }
    /// <summary>
    /// 值
    /// </summary>
    public string? Value { get; }

}