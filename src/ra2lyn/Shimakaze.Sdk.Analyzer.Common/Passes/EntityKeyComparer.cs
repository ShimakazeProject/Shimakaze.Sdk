namespace Shimakaze.Sdk.Analyzer.Common.Passes;

/// <summary>
/// 实体键比较器：组名与实体名均按 <see cref="StringComparer.OrdinalIgnoreCase"/> 比较。
/// </summary>
internal sealed class EntityKeyComparer : IEqualityComparer<(string Group, string Name)>
{
    /// <inheritdoc/>
    public bool Equals((string Group, string Name) left, (string Group, string Name) right)
        => string.Equals(left.Group, right.Group, StringComparison.OrdinalIgnoreCase)
        && string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public int GetHashCode((string Group, string Name) value)
        => HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(value.Group),
            StringComparer.OrdinalIgnoreCase.GetHashCode(value.Name));
}