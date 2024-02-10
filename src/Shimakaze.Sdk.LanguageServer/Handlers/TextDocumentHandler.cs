using System.Text.Json;

using MediatR;

using Microsoft.Extensions.Logging;

using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

using Shimakaze.Sdk.LanguageServer.Constants;
using Shimakaze.Sdk.LanguageServer.Models;
using Shimakaze.Sdk.LanguageServer.Services;
using Shimakaze.Sdk.LanguageServer.Services.Ini;

namespace Shimakaze.Sdk.LanguageServer.Handlers;

internal sealed partial class TextDocumentHandler(
    IniService iniService,
    DataManager dataManager,
    ILogger<TextDocumentHandler> logger
    ) : TextDocumentSyncHandlerBase
{
    private readonly ILogger<TextDocumentHandler> _logger = logger;

    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
    {
        var context = dataManager.Context.GetOrAdd(uri, uri => new()
        {
            Attribute = new(uri, LanguageIds.Text)
        });
        return context.Attribute;
    }

    public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        LogParams("didChange", JsonSerializer.Serialize(request, JsonSerializerOptions));
        switch (request.TextDocument.LanguageId)
        {
            case LanguageIds.Ini:
                iniService.Open(request.TextDocument.Uri, request.TextDocument.Text, request.TextDocument.Version);
                break;
            default:
                dataManager.Context.AddOrUpdate(
                    request.TextDocument.Uri,
                    uri => new()
                    {
                        Attribute = new(uri, request.TextDocument.LanguageId),
                        Text = request.TextDocument.Text,
                        Version = request.TextDocument.Version
                    },
                    (uri, old) => old
                );
                break;
        }
        return Unit.Task;
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        LogParams("didChange", JsonSerializer.Serialize(request, JsonSerializerOptions));
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        LogParams("didSave", JsonSerializer.Serialize(request, JsonSerializerOptions));
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        LogParams("didClose", JsonSerializer.Serialize(request, JsonSerializerOptions));
        return Unit.Task;
    }

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return new(TextDocumentSyncKind.Full)
        {
            DocumentSelector = TextDocumentSelector.ForLanguage("ini")
        };
    }

    [LoggerMessage(LogLevel.Debug, "textDocument/{path}\n{json}")]
    private partial void LogParams(string path, string json);
}
