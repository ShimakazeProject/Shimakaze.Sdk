namespace Shimakaze.Sdk.Inilyn.Syntax;

/// <summary>
/// INI 语法节点类型。
/// </summary>
public enum IniSyntaxKind
{
    // Token 类型映射
    LeftBracketToken,
    RightBracketToken,
    EqualToken,
    ColonToken,
    CommaToken,
    CommentTrivia,
    DocCommentTrivia,
    WhitespaceTrivia,
    NewlineTrivia,
    EndOfFileToken,
    PreprocessorDirectiveToken,
    StringToken,
    BadToken,

    // 复合节点
    CompilationUnit,
    SectionDeclaration,
    KeyValueEntry,
    SectionName,
    MixinReference,
    MixinReferenceList,
    PreprocessorDirective,
}
