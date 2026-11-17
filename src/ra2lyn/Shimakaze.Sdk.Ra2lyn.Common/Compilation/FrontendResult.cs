using Shimakaze.Sdk.Ra2lyn.Common.Ir;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Common.Compilation;

/// <summary>
/// 前端结果：源语言前端交给后端的产物。
/// </summary>
/// <param name="Group">前端结果所属的规则组。</param>
/// <param name="Sources">前端读取的源文件。</param>
/// <param name="Entities">前端产出的 IR 实体，尚未跨文件合并。</param>
/// <param name="Diagnostics">前端诊断。</param>
public sealed record class FrontendResult(
    string Group,
    IReadOnlyList<SourceFile> Sources,
    IReadOnlyList<EntityModel> Entities,
    IReadOnlyList<Diagnostic> Diagnostics);