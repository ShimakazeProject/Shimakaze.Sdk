using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Source;
using Shimakaze.Sdk.Ra2lyn.Ini.Lexing;

namespace Shimakaze.Sdk.Ra2lyn.Ini.Directives;

/// <summary>
/// 预处理器：识别指令行，按宏集合求值条件表达式，并在记号流上剔除未生效的内容。
/// </summary>
/// <remarks>
/// <para>
/// 结构性语义与 C# 一致：条件段与区域共用同一个分层栈，只能在栈顶打开新的 <c>#if</c> 或 <c>#region</c>，
/// <c>#elif</c>/<c>#else</c>/<c>#endif</c>/<c>#endregion</c> 只能关闭栈顶同类结构，
/// 因此 <c>#region</c> 不能跨越条件段边界，条件段也不能跨越区域边界。
/// </para>
/// <para>
/// 未生效部分的记号被剔除；其中的指令仍参与结构配对（例如未生效 <c>#if</c> 内部必须有对应的 <c>#endif</c>），
/// 这样与 C# 的行为保持一致。指令行本身永远不进入后续语法分析。
/// </para>
/// </remarks>
public static class IniPreprocessor
{
    /// <summary>
    /// 处理源文件。
    /// </summary>
    /// <param name="source">源文件。</param>
    /// <param name="defines">已定义的宏。</param>
    public static PreprocessResult Process(SourceFile source, DefineSet defines)
    {
        string text = source.Text;
        string path = source.Path;

        ConditionEvaluator evaluator = new(defines);
        List<Diagnostic> diagnostics = [];
        List<Line> lines = SplitLines(text, path);
        HashSet<int> directiveLineStarts = [];
        HashSet<int> activeLineStarts = [];
        List<Frame> stack = [];

        foreach (var line in lines)
        {
            string trimmed = line.Content.TrimStart();
            if (trimmed.Length is 0)
            {
                if (IsGloballyActive(stack))
                    activeLineStarts.Add(line.Start);

                continue;
            }

            if (trimmed.StartsWith('#'))
            {
                directiveLineStarts.Add(line.Start);
                HandleDirective(line, trimmed);
                continue;
            }

            if (IsGloballyActive(stack))
                activeLineStarts.Add(line.Start);
        }

        foreach (var frame in stack)
        {
            diagnostics.Add(new(
                DiagnosticCodes.DirectiveStructure,
                DiagnosticSeverity.Error,
                frame.Kind is FrameKind.Condition ? "#if 没有对应的 #endif" : "#region 没有对应的 #endregion",
                frame.Opening));
        }

        List<IniToken> tokens = [];
        foreach (var token in IniLexer.Tokenize(text, path))
        {
            if (token.Type is IniTokenType.EndOfFile)
            {
                tokens.Add(token);
                continue;
            }

            int lineStart = lineStartOf(token.Span.Start);
            if (directiveLineStarts.Contains(lineStart) || activeLineStarts.Contains(lineStart) is false)
                continue;

            tokens.Add(token);
        }

        return new([.. tokens], diagnostics);

        void HandleDirective(Line line, string trimmed)
        {
            string rest = trimmed[1..].TrimStart();
            int keywordLength = 0;
            while (keywordLength < rest.Length && (char.IsLetterOrDigit(rest[keywordLength]) || rest[keywordLength] is '_'))
                keywordLength++;

            string keyword = rest[..keywordLength];
            string operand = rest[keywordLength..].Trim();
            SourceSpan operandSpan = operand.Length is 0
                ? new(path, line.End, line.End)
                : OperandSpan(line, text, operand);

            SourceSpan lineSpan = new(path, line.Range.Start, line.End);
            if (IsInstruction(keyword, out DirectiveKind kind) is false)
            {
                diagnostics.Add(new(DiagnosticCodes.UnknownDirective, DiagnosticSeverity.Error, $"无法识别的预处理器指令 '#{keyword}'", lineSpan));
                if (IsGloballyActive(stack))
                    activeLineStarts.Add(line.Start);

                return;
            }

            Execute(kind);
            if (IsGloballyActive(stack))
                activeLineStarts.Add(line.Start);

            return;

            void Execute(DirectiveKind directiveKind)
            {
                switch (directiveKind)
                {
                    case DirectiveKind.If:
                        stack.Add(Frame.CreateCondition(lineSpan, Evaluate(operand, operandSpan, "#if", lineSpan)));
                        return;
                    case DirectiveKind.Elif:
                        ApplyElif();
                        return;
                    case DirectiveKind.Else:
                        ApplyElse();
                        return;
                    case DirectiveKind.EndIf:
                        ApplyEndIf();
                        return;
                    case DirectiveKind.Region:
                        stack.Add(Frame.CreateRegion(lineSpan));
                        return;
                    case DirectiveKind.EndRegion:
                        ApplyEndRegion();
                        return;
                }
            }

            bool Evaluate(string expression, SourceSpan expressionSpan, string directiveName, SourceSpan directiveSpan)
            {
                if (expression.Length is 0)
                {
                    diagnostics.Add(new(DiagnosticCodes.DirectiveStructure, DiagnosticSeverity.Error, $"{directiveName} 缺少条件表达式", directiveSpan));
                    return false;
                }

                ConditionResult result = evaluator.Evaluate(expression);
                if (result.Succeeded)
                    return result.Value;

                int errorOffset = result.ErrorOffset;
                string errorMessage = result.ErrorMessage;

                SourceSpan span = errorOffset >= 0 && errorOffset < expression.Length
                    ? new(path, expressionSpan.Start + errorOffset, expressionSpan.Start + errorOffset + 1)
                    : expressionSpan;
                diagnostics.Add(new(DiagnosticCodes.DirectiveExpression, DiagnosticSeverity.Error, errorMessage, span));
                return false;
            }

            void ApplyElif()
            {
                if (stack is not [.., { Kind: FrameKind.Condition } frame] || frame.SeenElse)
                {
                    diagnostics.Add(new(DiagnosticCodes.DirectiveStructure, DiagnosticSeverity.Error, "#elif 没有对应的 #if", lineSpan));
                    return;
                }

                bool condition = Evaluate(operand, operandSpan, "#elif", lineSpan);
                frame.Current = frame.BranchTaken is false && condition;
                frame.BranchTaken |= condition;
            }

            void ApplyElse()
            {
                if (stack is not [.., { Kind: FrameKind.Condition } frame] || frame.SeenElse)
                {
                    diagnostics.Add(new(DiagnosticCodes.DirectiveStructure, DiagnosticSeverity.Error, "#else 没有对应的 #if", lineSpan));
                    return;
                }

                frame.SeenElse = true;
                frame.Current = frame.BranchTaken is false;
                frame.BranchTaken = true;
            }

            void ApplyEndIf()
            {
                if (stack is not [.., { Kind: FrameKind.Condition }])
                {
                    diagnostics.Add(new(DiagnosticCodes.DirectiveStructure, DiagnosticSeverity.Error, "#endif 没有对应的 #if", lineSpan));
                    return;
                }

                stack.RemoveAt(stack.Count - 1);
            }

            void ApplyEndRegion()
            {
                if (stack is not [.., { Kind: FrameKind.Region }])
                {
                    diagnostics.Add(new(DiagnosticCodes.DirectiveStructure, DiagnosticSeverity.Error, "#endregion 没有对应的 #region", lineSpan));
                    return;
                }

                stack.RemoveAt(stack.Count - 1);
            }
        }

        int lineStartOf(int offset)
        {
            // 行首即“上一个换行之后的第一个字符”；按行表定位，避免再次扫描全文。
            int low = 0;
            int high = lines.Count - 1;
            while (low <= high)
            {
                int middle = (low + high) / 2;
                Line candidate = lines[middle];
                if (offset < candidate.Range.Start)
                    high = middle - 1;
                else if (offset >= candidate.Range.End && middle < lines.Count - 1)
                    low = middle + 1;
                else
                    return candidate.Range.Start;
            }

            return lines.Count is 0 ? 0 : lines[^1].Range.Start;
        }
    }

    private static bool IsInstruction(string keyword, out DirectiveKind kind)
    {
        switch (keyword)
        {
            case "if": kind = DirectiveKind.If; return true;
            case "elif": kind = DirectiveKind.Elif; return true;
            case "else": kind = DirectiveKind.Else; return true;
            case "endif": kind = DirectiveKind.EndIf; return true;
            case "region": kind = DirectiveKind.Region; return true;
            case "endregion": kind = DirectiveKind.EndRegion; return true;
            default: kind = default; return false;
        }
    }

    private static bool IsGloballyActive(List<Frame> stack)
    {
        foreach (var frame in stack)
        {
            if (frame.Current is false)
                return false;
        }

        return true;
    }

    private static SourceSpan OperandSpan(Line line, string text, string operand)
    {
            int index = text.IndexOf(operand, line.Range.Start, line.End - line.Range.Start, StringComparison.Ordinal);
        return index < 0
                ? new(line.Range.Path, line.End, line.End)
            : new(line.Range.Path, index, index + operand.Length);
    }

    private static List<Line> SplitLines(string text, string path)
    {
        List<Line> lines = [];
        int start = 0;
        for (int index = 0; index < text.Length; index++)
        {
            if (text[index] is not ('\r' or '\n'))
                continue;

            int contentEnd = index;
            int next = text[index] is '\r' && index + 1 < text.Length && text[index + 1] is '\n'
                ? index + 2
                : index + 1;

            // Range 覆盖包含换行符在内的整行，这样换行记号也会落在本行而不是下一行。
            lines.Add(new Line(new(path, start, next), contentEnd, text[start..contentEnd]));
            start = next;
            index = next - 1;
        }

        if (start < text.Length || lines.Count is 0)
            lines.Add(new Line(new(path, start, text.Length), text.Length, text[start..]));

        return lines;
    }

    private sealed class Line(SourceSpan range, int contentEnd, string content)
    {
        public SourceSpan Range { get; } = range;
        public int ContentEnd { get; } = contentEnd;
        public string Content { get; } = content;
        public int Start
            => Range.Start;
        public int End
            => ContentEnd;
    }

    private sealed class Frame(FrameKind kind, SourceSpan opening, bool branchTaken, bool current, bool seenElse)
    {
        public FrameKind Kind { get; } = kind;
        public SourceSpan Opening { get; } = opening;
        public bool BranchTaken { get; set; } = branchTaken;
        public bool Current { get; set; } = current;
        public bool SeenElse { get; set; } = seenElse;

        public static Frame CreateCondition(SourceSpan opening, bool condition)
            => new(FrameKind.Condition, opening, condition, condition, false);

        public static Frame CreateRegion(SourceSpan opening)
            => new(FrameKind.Region, opening, true, true, false);
    }

    private enum FrameKind
    {
        Condition,
        Region,
    }
}

/// <summary>
/// 预处理结果。
/// </summary>
/// <param name="Tokens">剔除未生效内容后的记号流，末尾仍是文件结束记号。</param>
/// <param name="Diagnostics">指令相关的诊断。</param>
public sealed record class PreprocessResult(
    IReadOnlyList<IniToken> Tokens,
    IReadOnlyList<Diagnostic> Diagnostics);