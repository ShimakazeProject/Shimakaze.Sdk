using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Ini.Syntax;

/// <summary>
/// 两个方括号之间名称在源文本中实际占据的范围（已裁剪首尾空白），供语法节点定位使用。
/// </summary>
/// <param name="Text">裁剪后的名称文本。</param>
/// <param name="Span">名称范围；名称为空时是零宽范围。</param>
internal readonly record struct NamedText(string Text, SourceSpan Span);
