using Shimakaze.Sdk.Ra2lyn.Common.Compilation;
using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Common.Emit;

/// <summary>
/// 发射器契约：把 <see cref="CompilationModel"/> 转换为具体格式的产物。
/// </summary>
public interface IEmitter
{
/// <summary>
    /// 执行发射；不支持的选项（如保留注释）必须报错，禁止静默忽略。
    /// </summary>
    EmitResult Emit(CompilationModel model, EmitOptions options);
}

/// <summary>
/// 发射选项。
/// </summary>
public sealed record class EmitOptions
{
    /// <summary>
    /// 是否丢弃不可达实体。
    /// </summary>
    public bool TreeShaking { get; init; }

    /// <summary>
    /// 是否保留注释；MVP 的 INI 发射器不支持该选项。
    /// </summary>
    public bool PreserveComments { get; init; }

    /// <summary>
    /// 输出使用的换行符。
    /// </summary>
    public string NewLine { get; init; } = "\r\n";
}

/// <summary>
/// 发射结果。
/// </summary>
/// <param name="Files">输出文件名到文本内容的映射。</param>
/// <param name="Diagnostics">发射阶段产生的诊断。</param>
public sealed record class EmitResult(
    IReadOnlyDictionary<string, string> Files,
    IReadOnlyList<Diagnostic> Diagnostics);
