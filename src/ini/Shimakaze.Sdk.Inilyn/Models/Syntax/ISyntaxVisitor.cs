namespace Shimakaze.Sdk.Inilyn.Models.Syntax;

/// <summary>
/// 表示语法访问器接口，用于实现对红树语法节点的访问和操作。
/// 通过访问者模式（Visitor Pattern），可以方便地对语法树进行遍历、分析或转换。
/// </summary>
public interface ISyntaxVisitor
{
    /// <summary>
    /// 访问键语法节点。
    /// </summary>
    /// <param name="node">要访问的键语法节点。</param>
    void Visit(KeySyntaxNode node);

    /// <summary>
    /// 访问值语法节点。
    /// </summary>
    /// <param name="node">要访问的值语法节点。</param>
    void Visit(ValueSyntaxNode node);

    /// <summary>
    /// 访问普通注释语法节点。
    /// </summary>
    /// <param name="node">要访问的注释语法节点。</param>
    void Visit(CommentSyntaxNode node);

    /// <summary>
    /// 访问文档注释语法节点。
    /// </summary>
    /// <param name="node">要访问的文档注释语法节点。</param>
    void Visit(DocumentCommentSyntaxNode node);

    /// <summary>
    /// 访问文档注释块语法节点。
    /// </summary>
    /// <param name="documentCommentBlockSyntaxNode">要访问的文档注释块语法节点。</param>
    void Visit(DocumentCommentBlockSyntaxNode documentCommentBlockSyntaxNode);

    /// <summary>
    /// 访问编译器指令语法节点。
    /// </summary>
    /// <param name="node">要访问的编译器指令语法节点。</param>
    void Visit(CompilerCommandSyntaxNode node);

    /// <summary>
    /// 访问节名称语法节点。
    /// </summary>
    /// <param name="node">要访问的节名称语法节点。</param>
    void Visit(SectionNameSyntaxNode node);

    /// <summary>
    /// 访问继承节名称语法节点。
    /// </summary>
    /// <param name="node">要访问的继承节名称语法节点。</param>
    void Visit(InheritSectionNameSyntaxNode node);

    /// <summary>
    /// 访问键值对语法节点。
    /// </summary>
    /// <param name="node">要访问的键值对语法节点。</param>
    void Visit(KeyValuePairSyntaxNode node);

    /// <summary>
    /// 访问节头语法节点。
    /// </summary>
    /// <param name="node">要访问的节头语法节点。</param>
    void Visit(SectionHeaderSyntaxNode node);

    /// <summary>
    /// 访问节数据语法节点。
    /// </summary>
    /// <param name="node">要访问的节数据语法节点。</param>
    void Visit(SectionDataSyntaxNode node);

    /// <summary>
    /// 访问节语法节点。
    /// </summary>
    /// <param name="node">要访问的节语法节点。</param>
    void Visit(SectionSyntaxNode node);

    /// <summary>
    /// 访问整个 INI 文档的根语法节点。
    /// </summary>
    /// <param name="node">要访问的文档语法节点。</param>
    void Visit(DocumentSyntaxNode node);
}
