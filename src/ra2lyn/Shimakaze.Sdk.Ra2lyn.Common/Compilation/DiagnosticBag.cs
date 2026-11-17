using Shimakaze.Sdk.Ra2lyn.Common.Source;

namespace Shimakaze.Sdk.Ra2lyn.Common.Compilation;

/// <summary>
/// 诊断收集器：去重并保持稳定的输出顺序。
/// </summary>
public sealed class DiagnosticBag : List<Diagnostic>
{
    /// <summary>
    /// 添加一条诊断；完全相同的诊断会被忽略。
    /// </summary>
    public new void Add(Diagnostic diagnostic)
    {
        if (this.Contains(diagnostic))
            return;

        base.Add(diagnostic);
    }

    /// <summary>
    /// 按文件顺序、偏移量、诊断码排序，得到稳定的输出。
    /// </summary>
    public IReadOnlyList<Diagnostic> ToSortedDiagnostics(IReadOnlyDictionary<string, int>? pathOrder = null)
    {
        List<Diagnostic> sorted = [.. this];
        sorted.Sort((left, right) =>
        {
            int leftOrder = GetPathOrder(left.Span.Path, pathOrder);
            int rightOrder = GetPathOrder(right.Span.Path, pathOrder);
            int result = leftOrder.CompareTo(rightOrder);
            if (result is not 0)
                return result;

            result = left.Span.Start.CompareTo(right.Span.Start);
            if (result is not 0)
                return result;

            result = left.Span.End.CompareTo(right.Span.End);
            if (result is not 0)
                return result;

            result = string.CompareOrdinal(left.Code, right.Code);
            return result is not 0 ? result : string.CompareOrdinal(left.Message, right.Message);
        });
        return sorted;
    }

    private static int GetPathOrder(string? path, IReadOnlyDictionary<string, int>? pathOrder)
        => path is not null && pathOrder is not null && pathOrder.TryGetValue(path, out int order)
        ? order
        : int.MaxValue;
}
