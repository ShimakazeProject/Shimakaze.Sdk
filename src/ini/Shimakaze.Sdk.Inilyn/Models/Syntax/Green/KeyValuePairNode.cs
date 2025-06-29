using LSPRange = Draco.Lsp.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

/// <summary>
/// 键值对节点
/// </summary>
/// <param name="key"></param>
/// <param name="value"></param>
/// <param name="comment"></param>
internal sealed class KeyValuePairNode(KeyNode key, ValueNode? value = null, CommentNode? comment = null) : GreenNode
{
    public override SyntaxKind Kind => SyntaxKind.KeyValuePair;

    public override LSPRange Range => CombineRange(key.Range, value?.Range, comment?.Range);

    public KeyNode Key => key;
    public ValueNode? Value => value;
    public CommentNode? Comment => comment;

    public override IEnumerable<GreenNode> GetChildren()
    {
        yield return key;
        if (value is not null)
            yield return value;
        if (comment is not null)
            yield return comment;
    }
}
