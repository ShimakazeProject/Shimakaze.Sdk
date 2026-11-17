using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Ini.Syntax;

/// <summary>
/// 一条无关记号（注释、空白或换行）。
/// </summary>
/// <param name="Kind">无关记号的种类。</param>
/// <param name="Text">原始文本。</param>
/// <param name="Span">记号在源文件中的位置，终点为开区间。</param>
public sealed record class Trivia(TriviaKind Kind, string Text, SourceSpan Span);
