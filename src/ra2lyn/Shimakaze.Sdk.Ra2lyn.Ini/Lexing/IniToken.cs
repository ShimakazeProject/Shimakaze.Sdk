using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Ini.Lexing;

/// <summary>
/// 一个词法记号。
/// </summary>
/// <param name="Type">记号类型。</param>
/// <param name="Text">记号的原始词素文本。</param>
/// <param name="Span">记号在源文件中的位置，终点为开区间。</param>
public sealed record class IniToken(IniTokenType Type, string Text, SourceSpan Span)
{
    /// <summary>
    /// 该记号能否作为键值对的起点：文本片段，或带 <c>:</c> 前缀的键（如 <c>:SecretBuilding</c>）。
    /// </summary>
    public bool IsKeyStart
        => Type is IniTokenType.Text or IniTokenType.Colon or IniTokenType.EqualSign;
}
