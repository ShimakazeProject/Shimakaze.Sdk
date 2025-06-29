using Draco.Lsp.Model;

namespace Shimakaze.Sdk.Inilyn.Text;

internal sealed class FullSourceText(string content, DocumentUri uri)
    : SourceText(CalculateLineStarts(content), uri)
{
    public override SourceText Root => this;

    public override int Length => content.Length;

    public override int StartIndex => 0;

    public override int EndIndex => Length;

    public override char this[Index index] => content[index];

    public override string ToString() => content;

    public override ReadOnlySpan<char> AsSpan() => ToString();
}
