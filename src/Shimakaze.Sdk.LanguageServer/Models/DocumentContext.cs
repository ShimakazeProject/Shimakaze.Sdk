using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Shimakaze.Sdk.LanguageServer.Models;

/// <summary>
/// 文档数据上下文
/// </summary>
internal sealed class DocumentContext
{
    public required TextDocumentAttributes Attribute { get; set; }
    public string? Text { get; set; }
    public int? Version { get; set; }
    public SemanticTokensDocument? SemanticTokensDocument { get; set; }
    public List<FoldingRange>? FoldingRanges { get; set; }
}
