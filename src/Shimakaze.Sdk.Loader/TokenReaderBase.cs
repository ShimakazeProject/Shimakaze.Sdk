namespace Shimakaze.Sdk.Loader;

public abstract class TokenReaderBase<TToken> : ITokenReader<TToken>
    where TToken : Enum
{
    protected readonly TextReader _reader;

    protected TokenReaderBase(TextReader reader)
    {
        this._reader = reader;
    }

    public abstract string Value { get; }
    public TToken? Token { get; protected set; }
    public abstract bool Read();
    
    protected void ReadAllWhiteSpaceChar()
    {
        while (_reader.Peek() is '\x20' or '\t' or '\r' or '\n')
            _reader.Read();
    }
}