using System.Diagnostics.CodeAnalysis;

namespace Shimakaze.Sdk.Ini;

/// <summary>
/// 表示一个INI节
/// </summary>
public interface IIniSection : IEnumerable<KeyValuePair<string, string>>
{
    /// <summary>
    /// 获取或设置一个键
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    string this[string key] { get; set; }

    /// <summary>
    /// 键值对数量
    /// </summary>
    int Count { get; }

    /// <summary>
    /// 节名
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 获取所有的键
    /// </summary>
    ICollection<string> Keys { get; }

    /// <summary>
    /// 获取所有的值
    /// </summary>
    ICollection<string> Values { get; }

    /// <summary>
    /// 添加一个键值对
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    void Add(string key, string value);

    /// <summary>
    /// 清空节
    /// </summary>
    void Clear();

    /// <summary>
    /// 是否包含键
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    bool ContainsKey(string key);

    /// <summary>
    /// 移除一个键值对
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    bool Remove(string key);

    /// <summary>
    /// 尝试获取一个值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    bool TryGetValue(string key, [MaybeNullWhen(false)] out string value);
}