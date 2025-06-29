using LSPRange = Draco.Lsp.Model.Range;

namespace Shimakaze.Sdk.Inilyn.Models.Syntax.Green;

/// <summary>
/// 绿树节点
/// </summary>
internal abstract class GreenNode
{
    public abstract SyntaxKind Kind { get; }

    public abstract LSPRange Range { get; }

    public virtual IEnumerable<GreenNode> GetChildren()
    {
        yield break;
    }

    public static LSPRange CombineRange(params IEnumerable<LSPRange?> ranges)
    {
        var data = ranges.OfType<LSPRange>();
        if (!data.Any())
            throw new ArgumentException("不可为空", nameof(ranges));

        var start = data.First().Start;
        var end = data.First().End;
        foreach (var range in data)
        {
            if (start.Line > range.Start.Line)
                start = range.Start;
            else if (start.Line == range.Start.Line && start.Character > range.Start.Character)
                start = range.Start;

            if (end.Line < range.End.Line)
                end = range.End;
            else if (end.Line == range.End.Line && end.Character < range.End.Character)
                end = range.End;
        }

        return new()
        {
            Start = start,
            End = end,
        };
    }
}
