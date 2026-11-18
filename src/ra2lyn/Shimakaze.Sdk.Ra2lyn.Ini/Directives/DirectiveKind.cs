namespace Shimakaze.Sdk.Ra2lyn.Ini.Directives;

/// <summary>
/// 预处理器指令种类。
/// </summary>
public enum DirectiveKind
{
    /// <summary>
    /// <c>#if 表达式</c>：开启一个条件段。
    /// </summary>
    If,

    /// <summary>
    /// <c>#elif 表达式</c>：条件段的后续分支。
    /// </summary>
    Elif,

    /// <summary>
    /// <c>#else</c>：条件段的兜底分支。
    /// </summary>
    Else,

    /// <summary>
    /// <c>#endif</c>：结束条件段。
    /// </summary>
    EndIf,

    /// <summary>
    /// <c>#region</c>：开启一个命名区域，只参与结构配对，不影响是否生效。
    /// </summary>
    Region,

    /// <summary>
    /// <c>#endregion</c>：结束命名区域。
    /// </summary>
    EndRegion,
}
