using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// 表示一个INI文档
/// </summary>
public interface IIniDocument<TIniSection> : IEnumerable<TIniSection>
    where TIniSection : IIniSection
{
    /// <summary>
    /// 获取或设置一个节
    /// </summary>
    /// <remarks>
    /// 注意：这并不会合并一个节
    /// </remarks>
    /// <param name="key">键</param>
    /// <returns>INI节</returns>
    TIniSection this[string key] { get; set; }

    /// <summary>
    /// 文档中节的数量
    /// </summary>
    int Count { get; }

    /// <summary>
    /// 在文档头部的没有包含在Section中的孤立的键值对
    /// </summary>
    TIniSection Default { get; }

    /// <summary>
    /// 获取所有的节的名字
    /// </summary>
    ICollection<string> SectionNames { get; }

    /// <summary>
    /// 获取所有的节
    /// </summary>
    ICollection<TIniSection> Sections { get; }

    /// <summary>
    /// 添加一个节
    /// </summary>
    /// <param name="item"></param>
    void Add(TIniSection item);

    /// <summary>
    /// 清空节内容
    /// </summary>
    void Clear();

    /// <summary>
    /// 是否包含节
    /// </summary>
    bool ContainsSection(string sectionName);

    /// <summary>
    /// 移除一个节
    /// </summary>
    bool Remove(string key);

    /// <summary>
    /// 尝试获取一个节
    /// </summary>
    bool TryGetSection(string key, [MaybeNullWhen(false)] out TIniSection section);
}