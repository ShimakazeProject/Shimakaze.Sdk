namespace Shimakaze.Sdk.Loader;

public interface ITokenReader<out TToken>
    where TToken : Enum
{
    string Value { get; }
    TToken? Token { get; }
    bool Read();
}