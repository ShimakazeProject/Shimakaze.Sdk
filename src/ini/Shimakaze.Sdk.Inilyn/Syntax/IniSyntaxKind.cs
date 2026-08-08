namespace Shimakaze.Sdk.Inilyn.Syntax;

/// <summary>
/// INI 语法节点类型。
/// </summary>
public enum IniSyntaxKind
{
    // Token 类型映射
    /// <summary>
    /// 左方括号 <c>[</c> 记号。
    /// </summary>
    LeftBracketToken,

    /// <summary>
    /// 右方括号 <c>]</c> 记号。
    /// </summary>
    RightBracketToken,

    /// <summary>
    /// 等号 <c>=</c> 记号。
    /// </summary>
    EqualToken,

    /// <summary>
    /// 冒号 <c>:</c> 记号。
    /// </summary>
    ColonToken,

    /// <summary>
    /// 逗号 <c>,</c> 记号。
    /// </summary>
    CommaToken,

    /// <summary>
    /// 注释琐碎内容。
    /// </summary>
    CommentTrivia,

    /// <summary>
    /// 文档注释琐碎内容。
    /// </summary>
    DocCommentTrivia,

    /// <summary>
    /// 空白琐碎内容。
    /// </summary>
    WhitespaceTrivia,

    /// <summary>
    /// 换行琐碎内容。
    /// </summary>
    NewlineTrivia,

    /// <summary>
    /// 文件结束记号。
    /// </summary>
    EndOfFileToken,

    /// <summary>
    /// 预处理指令记号。
    /// </summary>
    PreprocessorDirectiveToken,

    /// <summary>
    /// 字符串记号。
    /// </summary>
    StringToken,

    /// <summary>
    /// 非法记号。
    /// </summary>
    BadToken,

    // 复合节点
    /// <summary>
    /// 编译单元（整个文件的语法树根节点）。
    /// </summary>
    CompilationUnit,

    /// <summary>
    /// 节声明节点。
    /// </summary>
    SectionDeclaration,

    /// <summary>
    /// 键值对节点。
    /// </summary>
    KeyValueEntry,

    /// <summary>
    /// 节名节点。
    /// </summary>
    SectionName,

    /// <summary>
    /// Mixin 引用节点。
    /// </summary>
    MixinReference,

    /// <summary>
    /// Mixin 引用列表节点。
    /// </summary>
    MixinReferenceList,

    /// <summary>
    /// 预处理指令节点。
    /// </summary>
    PreprocessorDirective,
}
