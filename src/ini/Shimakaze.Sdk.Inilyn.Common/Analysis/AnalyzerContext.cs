namespace Shimakaze.Sdk.Inilyn.Analysis;

/// <summary>
/// 分析器管道上下文，携带分析过程中的共享状态。
/// </summary>
/// <param name="source">源文本。</param>
/// <param name="documentId">文档 ID。</param>
/// <param name="cancellationToken">取消令牌。</param>
public sealed class AnalyzerContext(
    string source,
    Guid documentId,
    CancellationToken cancellationToken = default)
{
    private Dictionary<string, object>? _properties;

    /// <summary>
    /// 源文本。
    /// </summary>
    public string Source { get; } = source;

    /// <summary>
    /// 文档 ID。
    /// </summary>
    public Guid DocumentId { get; } = documentId;

    /// <summary>
    /// 取消令牌。
    /// </summary>
    public CancellationToken CancellationToken { get; } = cancellationToken;

    /// <summary>
    /// 扩展属性字典，供分析器之间传递任意状态。
    /// </summary>
    public IReadOnlyDictionary<string, object> Properties
        => _properties ??= [];

    /// <summary>
    /// 获取指定键的扩展属性值。
    /// </summary>
    /// <typeparam name="T">值类型。</typeparam>
    /// <param name="key">属性键。</param>
    /// <returns>属性值，不存在或类型不匹配时返回 <see langword="null"/>。</returns>
    public T? GetProperty<T>(string key)
    {
        if (_properties is not null && _properties.TryGetValue(key, out object? value) && value is T typed)
            return typed;

        return default;
    }

    /// <summary>
    /// 设置指定键的扩展属性值。
    /// </summary>
    /// <param name="key">属性键。</param>
    /// <param name="value">属性值。</param>
    public void SetProperty(string key, object value)
    {
        _properties ??= [];
        _properties[key] = value;
    }
}
