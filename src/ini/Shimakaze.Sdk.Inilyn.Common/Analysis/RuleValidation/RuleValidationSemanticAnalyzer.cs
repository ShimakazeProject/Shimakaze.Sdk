using Microsoft.EntityFrameworkCore;

using Shimakaze.Sdk.Inilyn.Analysis.Semantic;
using Shimakaze.Sdk.Inilyn.Data;
using Shimakaze.Sdk.Inilyn.Data.Syntax;
using Shimakaze.Sdk.Inilyn.Model;

namespace Shimakaze.Sdk.Inilyn.Analysis.RuleValidation;

/// <summary>
/// 基于规则的语义分析器，验证 INI 节的键值对是否符合类型约束。
/// </summary>
/// <remarks>
/// <para>
/// 该分析器根据 XML 规则定义检查每个节的键值对：
/// </para>
/// <list type="bullet">
///   <item><description>验证键的值类型是否匹配声明的类型（int、float、boolean 等）。</description></item>
///   <item><description>验证复合类型（Vector2、Vector3）的字段数量和格式。</description></item>
///   <item><description>验证列表值的每个元素是否符合声明的类型。</description></item>
/// </list>
/// </remarks>
/// <param name="ruleSet">规则集。</param>
/// <param name="db">数据库上下文（用于获取节的分类信息）。</param>
public sealed class RuleValidationSemanticAnalyzer(RuleSet ruleSet, IniDbContext db) : ISemanticAnalyzer
{
    /// <inheritdoc/>
    public SemanticAnalysisResult Analyze(AnalyzerContext context, IReadOnlyList<SyntaxNode> nodes)
    {
        var result = new SemanticAnalysisResult();
        var sections = nodes.OfType<SectionNode>().ToList();

        foreach (var section in sections)
        {
            string groupName = GetGroupName(section, db);
            if (!ruleSet.Groups.TryGetValue(groupName, out var group))
                continue;

            var validator = new SectionRuleValidator(group);
            var diagnostics = validator.Validate(section);

            foreach (var diagnostic in diagnostics)
            {
                db.Diagnostics.Add(diagnostic);
            }
        }

        return result;
    }

    private static string GetGroupName(SectionNode section, IniDbContext db)
    {
        var document = db.Documents
            .Include(d => d.Category)
            .FirstOrDefault(d => d.Id == section.DocumentId);

        return document?.Category?.Name ?? string.Empty;
    }
}
