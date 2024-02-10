using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

using Shimakaze.Sdk.LanguageServer.Models;
using Shimakaze.Sdk.LanguageServer.Services;
using Shimakaze.Sdk.LanguageServer.Services.Ini;

namespace Shimakaze.Sdk.LanguageServer.Handlers;
internal sealed class SemanticTokensHandler(IniService iniParser, DataManager dataManager) : SemanticTokensHandlerBase
{
    protected override SemanticTokensRegistrationOptions CreateRegistrationOptions(SemanticTokensCapability capability, ClientCapabilities clientCapabilities)
    {
        return new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("ini"),
            Range = true,
            Full = new SemanticTokensCapabilityRequestFull()
            {
                Delta = true
            },
            Legend = iniParser.Legend,
            WorkDoneProgress = true,
        };
    }

    protected override Task<SemanticTokensDocument> GetSemanticTokensDocument(ITextDocumentIdentifierParams @params, CancellationToken cancellationToken)
    {

        if (dataManager.Context.TryGetValue(@params.TextDocument.Uri, out var context))
        {
            switch (context.Attribute.LanguageId)
            {
                case "ini":
                    context.SemanticTokensDocument ??= new(iniParser.Legend);
                    return Task.FromResult(context.SemanticTokensDocument);
                default:
                    break;
            }
        }

        throw new FileNotFoundException();
    }

    protected override Task Tokenize(SemanticTokensBuilder builder, ITextDocumentIdentifierParams identifier, CancellationToken cancellationToken)
    {
        if (dataManager.Context.TryGetValue(identifier.TextDocument.Uri, out var context))
        {
            switch (context.Attribute.LanguageId)
            {
                case "ini":
                    iniParser.Tokenize(builder, identifier.TextDocument.Uri);
                    context.SemanticTokensDocument = builder.Commit();
                    break;
                default:
                    break;
            }

        }

        return Task.CompletedTask;
    }
}
