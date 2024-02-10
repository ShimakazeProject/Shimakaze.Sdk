using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

using Shimakaze.Sdk.LanguageServer.Constants;
using Shimakaze.Sdk.LanguageServer.Services.Ini.Parser;

namespace Shimakaze.Sdk.LanguageServer.Models;
internal sealed class IniDocumentAttributes : TextDocumentAttributes
{
    public IniDocumentAttributes(DocumentUri uri, IniType type) : base(uri, LanguageIds.Ini)
    {
        Type = type;
    }

    public IniDocumentAttributes(DocumentUri uri, string scheme, IniType type) : base(uri, scheme, LanguageIds.Ini)
    {
        Type = type;
    }

    public IniType Type { get; set; }

    public List<IniSymbol> Tokens { get; } = [];

}
