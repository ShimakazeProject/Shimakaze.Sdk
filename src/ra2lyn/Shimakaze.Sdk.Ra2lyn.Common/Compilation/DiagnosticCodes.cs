namespace Shimakaze.Sdk.Ra2lyn.Common.Compilation;

/// <summary>
/// 诊断码定义：后端与前端共享的诊断代码常量。
/// </summary>
public static class DiagnosticCodes
{
    /// <summary>
    /// INI001：缺少左方括号。
    /// </summary>
    public const string MissingLeftBracket = "INI001";

    /// <summary>
    /// INI002：缺少右方括号。
    /// </summary>
    public const string MissingRightBracket = "INI002";

    /// <summary>
    /// INI003：缺少名称。
    /// </summary>
    public const string MissingName = "INI003";

    /// <summary>
    /// INI004：缺少继承冒号。
    /// </summary>
    public const string MissingColon = "INI004";

    /// <summary>
    /// INI005：缺少继承逗号。
    /// </summary>
    public const string MissingComma = "INI005";

    /// <summary>
    /// INI006：缺少等号。
    /// </summary>
    public const string MissingEquals = "INI006";

    /// <summary>
    /// INI007：缺少键名。
    /// </summary>
    public const string MissingKey = "INI007";

    /// <summary>
    /// INI008：值被意外截断或缺失。
    /// </summary>
    public const string MissingValue = "INI008";

    /// <summary>
    /// INI009：键名中包含空白。
    /// </summary>
    public const string WhitespaceInKey = "INI009";

    /// <summary>
    /// INI010：换行后的内容无法识别。
    /// </summary>
    public const string UnexpectedToken = "INI010";

    /// <summary>
    /// INI011：无法识别的预处理器指令。
    /// </summary>
    public const string UnknownDirective = "INI011";

    /// <summary>
    /// INI012：指令的结构不成立（缺少 #if 或 #endif、分支顺序错误、#region 嵌套错误等）。
    /// </summary>
    public const string DirectiveStructure = "INI012";

    /// <summary>
    /// INI013：条件表达式无法解析或求值。
    /// </summary>
    public const string DirectiveExpression = "INI013";

    /// <summary>
    /// INI014：<c>#pragma</c> 子指令的参数缺失、格式不合法，或子指令本身暂未支持。
    /// </summary>
    public const string PragmaArgument = "INI014";

    /// <summary>
    /// INI015：模板节头 <c>[$Name] :</c> 之后既不是 <c>[</c> 继承列表，也不是 <c>where</c> 关键字。
    /// </summary>
    public const string TemplateWhereExpected = "INI015";

    /// <summary>
    /// INI016：模板节头 <c>[$Name] : where</c> 之后缺少表达式。
    /// </summary>
    public const string TemplateWhereMissing = "INI016";

    /// <summary>
    /// INI017：模板 <c>where</c> 表达式无法解析，或使用了不支持的方法 / 函数 / 语法。
    /// </summary>
    public const string TemplateWhereSyntax = "INI017";

    /// <summary>
    /// INI018：模板 <c>where</c> 表达式在求值时出现 NullReferenceError（属性不存在却用单点 <c>.</c> 访问）。
    /// </summary>
    public const string TemplateWhereNullReference = "INI018";

    /// <summary>
    /// INI019：<c>$</c> 引用无法识别（未配对的 <c>${</c>、空表达式、<c>.</c> 之后又出现 <c>::</c> 等，TODO.md §3.5.6）。
    /// </summary>
    public const string VariableSyntax = "INI019";

    /// <summary>
    /// INI020：<c>$</c> 引用解析不到任何属性（TODO.md §3.5.6：不匹配的 <c>::</c> / <c>.</c> 链）。
    /// </summary>
    public const string UnresolvedVariable = "INI020";

    /// <summary>
    /// INI021：属性之间出现循环引用（TODO.md §3.4）。
    /// </summary>
    public const string CircularVariableReference = "INI021";

    /// <summary>
    /// INI022：求值递归深度超过 <c>CompilationOptions.MaxRecursionDepth</c>（TODO.md §3.4）。
    /// </summary>
    public const string VariableRecursionLimit = "INI022";

    /// <summary>
    /// INI023：同一条模板在多个前端里带了不同的 <c>where</c> 条件（模板节参与 MergePass，条件按 <c>&amp;&amp;</c> 合并）。
    /// </summary>
    public const string ConflictingTemplateWhere = "INI023";

    /// <summary>
    /// INI099：不可恢复的语法错误。
    /// </summary>
    public const string SyntaxError = "INI099";

    /// <summary>
    /// INI101：重复的节名。
    /// </summary>
    public const string DuplicateSection = "INI101";

    /// <summary>
    /// INI102：顶层键值对没有所属节。
    /// </summary>
    public const string KeyOutsideSection = "INI102";

    /// <summary>
    /// INI103：同一节内重复的键。
    /// </summary>
    public const string DuplicateKey = "INI103";

    /// <summary>
    /// INI104：无法解析的引用。
    /// </summary>
    public const string UnresolvedReference = "INI104";

    /// <summary>
    /// INI201：类型不匹配。
    /// </summary>
    public const string TypeMismatch = "INI201";

    /// <summary>
    /// INI202：未知的节定义。
    /// </summary>
    public const string UnknownDefinition = "INI202";

    /// <summary>
    /// INI203：未知的键。
    /// </summary>
    public const string UnknownKey = "INI203";
}
