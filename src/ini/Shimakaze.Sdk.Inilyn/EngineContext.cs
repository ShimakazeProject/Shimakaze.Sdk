using Draco.Lsp.Model;

namespace Shimakaze.Sdk.Inilyn;

/// <summary>
/// 引擎上下文
/// </summary>
public sealed class EngineContext
{
    private readonly List<Diagnostic> _diagnostics = [];

    /// <summary>
    /// 诊断信息
    /// </summary>
    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics.AsReadOnly();

    /// <summary>
    /// 添加诊断信息
    /// </summary>
    /// <param name="diagnostic"></param>
    public void Report(Diagnostic diagnostic)
    {
        _diagnostics.Add(diagnostic);
    }
}
