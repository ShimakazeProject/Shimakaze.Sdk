using System.Text;

using Shimakaze.Sdk.Inilyn.Text;

namespace Shimakaze.Sdk.Inilyn.Models.Token;

/// <summary>
/// INI Token
/// </summary>
/// <param name="Range">范围</param>
/// <param name="Type">类型</param>
/// <param name="Value">值</param>
public sealed record class IniToken(Draco.Lsp.Model.Range Range, IniTokenType Type, SourceText Value)
{
    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder sb = new();
        if (Range.Start.Line == Range.End.Line)
        {
            sb.Append(Range.Start.Line)
                .Append(':')
                .Append(Range.Start.Character)
                .Append('-')
                .Append(Range.End.Character);
        }
        else
        {
            sb.Append(Range.Start.Line)
                .Append(':')
                .Append(Range.Start.Character)
                .Append('-')
                .Append(Range.End.Line)
                .Append(':')
                .Append(Range.End.Character);
        }

        return $"{sb} {Type} \"{Value}\"";
    }
}
