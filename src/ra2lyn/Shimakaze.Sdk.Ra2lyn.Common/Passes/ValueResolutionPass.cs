using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Source;
using Shimakaze.Sdk.Ra2lyn.Common.Value;

namespace Shimakaze.Sdk.Ra2lyn.Common.Passes;

/// <summary>
/// 值替换 Pass：在所有合并与模板展开完成后，把属性值里的 <c>$</c> 引用替换成真实值
/// （TODO.md §2、§3、§5）。
/// </summary>
/// <remarks>
/// <para>
/// 执行位置：<see cref="TemplateExpansionPass"/> 之后、<c>NullCleanupPass</c> 之前（TODO.md §5）。
/// 此时模板 body 已经注入完毕（模板注入的 <c>$</c> 占位符也在这里统一解析，§4.4），
/// 因此分析器随后看到的就是最终真实值。
/// </para>
/// <para>
/// 处理内容：
/// </para>
/// <list type="bullet">
/// <item>三种 <c>$</c>（TODO.md §3.5.5）：花括号变量 <c>${Expr}</c>、普通变量 <c>$Expr</c>、
/// 裸占位符 <c>$</c>（"当前最终值"，§2）。</item>
/// <item>四种路径形式（TODO.md §3.5.1）：<c>Group::Section.Key</c> / <c>Section.Key</c> / <c>Key</c> /
/// <c>Group::Key</c>。</item>
/// <item>常量 <c>$null</c>（TODO.md §3.5.7）：<c>Key=$null</c> 的值在模型里被标记为 <see langword="null"/>，
/// 但不删除 key——删除由后续的 <c>NullCleanupPass</c> 完成（TODO.md §3.5.8）。</item>
/// </list>
/// <para>
/// 递归求值、循环引用检测与递归深度上限由 <see cref="ValueResolver"/> 负责（TODO.md §3.4）。
/// 求值失败的属性保留原始字符串，诊断换算回属性在源文件里的位置。
/// </para>
/// <para>
/// 模板节已在 <see cref="TemplateExpansionPass"/> 里整体剔除，本 Pass 不会再见到它们；
/// 残留的模板节会被跳过，不参与求值（TODO.md §4.5）。
/// </para>
/// </remarks>
public sealed class ValueResolutionPass : ICompilationPass
{
    private readonly ValueResolver _resolver = new();

    /// <inheritdoc/>
    public string Name => "ValueResolution";

    /// <inheritdoc/>
    public void Run(CompilationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Entities.Count is 0)
            return;

        IReadOnlyList<EntityModel> entities = context.Entities;
        ResolutionResult resolution = _resolver.Resolve(entities, context.Options.MaxRecursionDepth);

        foreach (var diagnostic in resolution.Diagnostics)
        {
            context.Diagnostics.Add(MakeDiagnostic(diagnostic, FindOrigin(entities, diagnostic.Owner)));
        }

        List<EntityModel> replaced = [];
        foreach (var entity in entities)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (entity.IsTemplate)
            {
                replaced.Add(entity);
                continue;
            }

            List<PropertyModel> properties = [];
            foreach (var property in entity.Properties)
            {
                PropertyAddress address = new(entity.Group, entity.Name, property.Key);
                if (resolution.Values.TryGetValue(address.Composite, out var resolved) is false
                    || resolved.Succeeded is false)
                {
                    // 未求值或求值失败：保留原始字符串（TODO.md §3.4）。
                    properties.Add(property);
                    continue;
                }

                properties.Add(Replace(property, resolved));
            }

            replaced.Add(new EntityModel
            {
                Id = entity.Id,
                Group = entity.Group,
                Name = entity.Name,
                BaseNames = entity.BaseNames,
                Properties = properties,
                Origin = entity.Origin,
                Priority = entity.Priority,
                IsTemplate = entity.IsTemplate,
                TemplateWhere = entity.TemplateWhere,
            });
        }

        context.SetEntities(replaced);
    }

    /// <summary>
    /// 把求值结果写回属性；<see cref="ResolvedValue.HasValue"/> 为 <see langword="false"/> 时值被标记为 null。
    /// </summary>
    /// <remarks>
    /// null 在模型里的表示是 <see cref="PropertyModel.Value"/> 为 <see langword="null"/>：
    /// 后续的 <c>NullCleanupPass</c> 据此识别并删除该 key（TODO.md §3.5.8）。
    /// 值的形态（引用值 / 简单值）与列表分隔符保持不变；History 链原样带走。
    /// </remarks>
    private static PropertyModel Replace(PropertyModel property, ResolvedValue resolved)
        => new()
        {
            Id = property.Id,
            Key = property.Key,
            Value = resolved.HasValue ? resolved.Text! : null!,
            ValueKind = property.ValueKind,
            ListSeparator = property.ListSeparator,
            TemplateName = property.TemplateName,
            Origin = property.Origin,
            History = property.History,
        };

    /// <summary>
    /// 在实体列表里找到诊断所属属性的源位置。
    /// </summary>
    /// <remarks>
    /// 模板注入的属性保留模板源文件里的位置（TODO.md §4.5），因此诊断会指回模板原位置。
    /// </remarks>
    private static SourceSpan FindOrigin(IReadOnlyList<EntityModel> entities, PropertyAddress address)
    {
        foreach (var entity in entities)
        {
            if (string.Equals(entity.Group, address.Group, StringComparison.OrdinalIgnoreCase) is false
                || string.Equals(entity.Name, address.Name, StringComparison.OrdinalIgnoreCase) is false)
            {
                continue;
            }

            foreach (var property in entity.Properties)
            {
                if (string.Equals(property.Key, address.Key, StringComparison.OrdinalIgnoreCase))
                    return property.Origin;
            }
        }

        return default;
    }

    /// <summary>
    /// 把相对属性值文本的偏移换算成源文件里的 <see cref="SourceSpan"/>。
    /// </summary>
    /// <remarks>
    /// 属性的 <see cref="PropertyModel.Origin"/> 从键起点覆盖到值终点，值文本位于该范围的末尾；
    /// 因此从范围末尾倒推 <c>属性值长度 - 偏移</c> 即得。范围缺失时退化为没有位置的诊断。
    /// </remarks>
    private static Diagnostic MakeDiagnostic(VariableDiagnostic diagnostic, SourceSpan origin)
    {
        string message = $"属性 '{diagnostic.Owner}' 的变量引用无法求值: {diagnostic.Message}";

        // FindOrigin 找到了属性，但值文本长度未知时只报告键范围。
        if (origin.IsEmpty)
            return new Diagnostic(diagnostic.Code, DiagnosticSeverity.Error, message, origin);

        // 值文本由 VariableReferenceParser 的行内偏移度量，键范围覆盖到值末尾，因此值文本
        // 必在 origin.End 之前：用一个保守下界裁剪到原范围即可。
        int start = Math.Max(origin.Start, origin.End - diagnostic.Length);
        int end = Math.Min(origin.End, start + Math.Max(1, diagnostic.Length));
        SourceSpan span = new(origin.Path, Math.Min(start, end), end);
        return new Diagnostic(diagnostic.Code, DiagnosticSeverity.Error, message, span);
    }
}
