using System.Text.RegularExpressions;

using Shimakaze.Sdk.Inilyn.Semantic;

namespace Shimakaze.Sdk.Inilyn.Compilation;

/// <summary>
/// 最终工作区结构：将多个文档（每个文件 Mixin 展开后的内存 INI）按文件名顺序合并的结果。
/// </summary>
/// <remarks>
/// <para>
/// 合并时支持 <c>$()</c> 引用语法，用于引用已合并到工作区的值而不破坏原有内容，例如
/// <c>BaseUnit=$(BaseUnit),SFMCV</c> 会引用当前节 <c>BaseUnit</c> 的原值并追加。
/// </para>
/// <para>
/// 引用路径：<c>$(Group.Section.Key)</c>、<c>$(Section.Key)</c> 或 <c>$(Key)</c>。
/// <c>Group</c> 可选（默认当前），<c>Section</c> 可选（默认当前），<c>Key</c> 必填。
/// </para>
/// </remarks>
public sealed class IniWorkspace
{
    private static readonly Regex VariableReference = new(@"\$\(([^)]*)\)", RegexOptions.Compiled);

    private readonly Dictionary<string, List<IniSemanticKeyValue>> _sections = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IniSemanticKeyValue> _globalKeys = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 将一个文档合并到工作区：同节的键合并，后者的键覆盖前者同名键（保留声明顺序）；空值移除键。
    /// 值中的 <c>$()</c> 引用会在合并时解析。
    /// </summary>
    /// <param name="document">待合并的文档（已 Mixin 展开）。</param>
    public void Merge(IniSemanticModel document)
    {
        foreach (var section in document.Sections)
        {
            if (_sections.TryGetValue(section.Name, out var existing))
            {
                MergeKeyValues(section.Name, existing, section.KeyValues);
            }
            else
            {
                // 新建节：逐项解析 $() 引用后写入
                _sections[section.Name] = section.KeyValues
                    .Select(kv => new IniSemanticKeyValue(kv.Key, ResolveValue(section.Name, kv.Value), kv.SourceSection))
                    .ToList();
            }
        }

        foreach (var kv in document.GlobalKeys)
        {
            _globalKeys[kv.Key] = new IniSemanticKeyValue(kv.Key, ResolveValue(null, kv.Value));
        }
    }

    /// <summary>
    /// 生成最终语义模型。
    /// </summary>
    /// <returns>合并后的语义模型。</returns>
    public IniSemanticModel ToModel()
    {
        var sections = _sections
            .Select(kv => new IniSemanticSection(kv.Key, kv.Value))
            .ToList();

        return new IniSemanticModel
        {
            Sections = sections,
            GlobalKeys = [.. _globalKeys.Values],
            Diagnostics = [],
        };
    }

    private void MergeKeyValues(string sectionName, List<IniSemanticKeyValue> existing, IReadOnlyList<IniSemanticKeyValue> incoming)
    {
        foreach (var kv in incoming)
        {
            string resolved = ResolveValue(sectionName, kv.Value);

            int index = existing.FindIndex(k => string.Equals(k.Key, kv.Key, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                if (string.IsNullOrEmpty(resolved))
                {
                    // 空值：覆盖并移除该键
                    existing.RemoveAt(index);
                }
                else
                {
                    // 后者覆盖前者，保留原位置
                    existing[index] = new IniSemanticKeyValue(kv.Key, resolved, kv.SourceSection);
                }
            }
            else if (!string.IsNullOrEmpty(resolved))
            {
                existing.Add(new IniSemanticKeyValue(kv.Key, resolved, kv.SourceSection));
            }
        }
    }

    private string ResolveValue(string? currentSection, string value)
    {
        if (!value.Contains("$("))
        {
            return value;
        }

        return VariableReference.Replace(value, m =>
        {
            string body = m.Groups[1].Value;
            string[] parts = body.Split('.');

            string? section;
            string key;
            switch (parts.Length)
            {
                case 1:
                    section = currentSection;
                    key = parts[0];
                    break;
                case 2:
                    section = parts[0];
                    key = parts[1];
                    break;
                default:
                    // Group.Section.Key：当前工作区为单个规则组，Group 视为当前组
                    section = parts[1];
                    key = string.Join(".", parts.Skip(2));
                    break;
            }

            return Lookup(section, key) ?? string.Empty;
        });
    }

    private string? Lookup(string? section, string key)
    {
        if (section is null || !_sections.TryGetValue(section, out var keyValues))
        {
            return null;
        }

        foreach (var kv in keyValues)
        {
            if (string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                return kv.Value;
            }
        }

        return null;
    }
}