using LSPRange = Draco.Lsp.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

/// <summary>
/// 表示整个 INI 文件的根节点。
/// </summary>
/// <param name="sections">文件中的所有节（包括虚拟节）</param>
internal sealed class DocumentNode(IEnumerable<SectionNode> sections) : GreenNode
{
    public override SyntaxKind Kind => SyntaxKind.Document;

    public override LSPRange Range => CombineRange(
        sections.Select(s => s.Range).OfType<LSPRange>());

    /// <summary>
    /// 获取文件中的所有节（包括虚拟节）
    /// </summary>
    public IReadOnlyList<SectionNode> Sections { get; } = [.. sections];

    public override IEnumerable<SectionNode> GetChildren() => Sections;
}
