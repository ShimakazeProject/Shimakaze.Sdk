namespace Shimakaze.Sdk.Inilyn.Models.Syntax;

/// <summary>
/// 表示 INI 文件中语法节点的种类。
/// </summary>
public enum SyntaxKind
{
    /// <summary>
    /// 整个文件的根节点。
    /// </summary>
    /// <remarks>
    /// <para>对应整个 INI 文件内容。</para>
    /// <para>包含多个 Section（包括虚拟节）。</para>
    /// </remarks>
    Document,

    /// <summary>
    /// 节点（Section），可以是虚拟节或真实节。
    /// </summary>
    /// <remarks>
    /// <para>表示一个节（[section] 或虚拟全局节）。</para>
    /// <para>包含可选的文档注释、可选的节头、以及必须的数据块。</para>
    /// </remarks>
    Section,

    /// <summary>
    /// 节的头部信息。
    /// </summary>
    /// <remarks>
    /// <para>用于描述节名及其可选的继承关系。</para>
    /// <para>例如：[section] : [base]</para>
    /// </remarks>
    SectionHeader,

    /// <summary>
    /// 节名称。
    /// </summary>
    /// <remarks>
    /// <para>节声明中的名称部分。</para>
    /// <para>例如：[section] 中的 "section"。</para>
    /// </remarks>
    SectionName,

    /// <summary>
    /// 继承的节名称。
    /// </summary>
    /// <remarks>
    /// <para>节声明中冒号后指定的基节名。</para>
    /// <para>例如：[derived] : [base] 中的 "base"。</para>
    /// </remarks>
    InheritSectionName,

    /// <summary>
    /// 文档注释（单行）。
    /// </summary>
    /// <remarks>
    /// <para>以三个分号开头的注释，作用于其后的节或文件整体。</para>
    /// <para>例如：;;; 这是一个文档注释</para>
    /// </remarks>
    DocumentComment,

    /// <summary>
    /// 文档注释块。
    /// </summary>
    /// <remarks>
    /// <para>由多个连续的 <see cref="DocumentComment"/> 组成的块。</para>
    /// <para>用于组织多个文档注释为一个逻辑单元。</para>
    /// </remarks>
    DocumentCommentBlock,

    /// <summary>
    /// 节的数据部分。
    /// </summary>
    /// <remarks>
    /// <para>节内部的内容集合，包含键值对、编译器命令和普通注释。</para>
    /// <para>每个节必须有一个数据块。</para>
    /// </remarks>
    SectionData,

    /// <summary>
    /// 键值对（key=value）。
    /// </summary>
    /// <remarks>
    /// <para>INI 文件中最基本的配置项。</para>
    /// <para>包含键、值以及可选的行尾注释。</para>
    /// </remarks>
    KeyValuePair,

    /// <summary>
    /// 键（Key）。
    /// </summary>
    /// <remarks>
    /// <para>键值对中的左侧部分。</para>
    /// <para>例如：key = value 中的 "key"</para>
    /// </remarks>
    Key,

    /// <summary>
    /// 值（Value）。
    /// </summary>
    /// <remarks>
    /// <para>键值对中的右侧部分。</para>
    /// <para>支持字符串、引号包裹、转义等。</para>
    /// </remarks>
    Value,

    /// <summary>
    /// 普通注释。
    /// </summary>
    /// <remarks>
    /// <para>以单个分号开头的注释。</para>
    /// <para>可以独立成行，也可以跟在键值对后面。</para>
    /// </remarks>
    Comment,

    /// <summary>
    /// 编译器指令（如 #include）。
    /// </summary>
    /// <remarks>
    /// <para>以井号开头的预处理指令。</para>
    /// <para>可以在任意位置出现。</para>
    /// </remarks>
    CompilerCommand,
}
