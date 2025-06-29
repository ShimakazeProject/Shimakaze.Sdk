using Draco.Lsp.Model;

namespace Shimakaze.Sdk.Inilyn;

/// <summary>
/// 引擎上下文
/// </summary>
public sealed class EngineContext
{
    /// <summary>
    /// 诊断信息
    /// </summary>
    public List<Diagnostic> Diagnostics => [];
}
