namespace Shimakaze.Sdk.Ra2lyn.Common.Compilation;

/// <summary>
/// 一个编译阶段。
/// </summary>
public interface ICompilationPass
{
    /// <summary>
    /// Pass 名称，用于诊断与调试。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 执行 Pass，读取并改写 <see cref="CompilationContext"/>。
    /// </summary>
    void Run(CompilationContext context);
}