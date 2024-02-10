using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

using Shimakaze.Sdk.LanguageServer.Services;
using Shimakaze.Sdk.LanguageServer.Services.Ini;

namespace Shimakaze.Sdk.LanguageServer.Handlers;

internal sealed class FoldingRangeHandler(DataManager dataManager) : FoldingRangeHandlerBase
{
    public override Task<Container<FoldingRange>?> Handle(FoldingRangeRequestParam request, CancellationToken cancellationToken)
    {
        switch (dataManager.Context[request.TextDocument.Uri].Attribute.LanguageId)
        {
            case "ini":
                return Task.FromResult(Container<FoldingRange>.From(dataManager.Context[request.TextDocument.Uri].FoldingRanges));
            default:
                break;
        }
        return Task.FromResult<Container<FoldingRange>?>(null);
    }

    protected override FoldingRangeRegistrationOptions CreateRegistrationOptions(FoldingRangeCapability capability, ClientCapabilities clientCapabilities)
    {
        return new()
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("ini"),
            WorkDoneProgress = true,
        };
    }
}
