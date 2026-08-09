using Shimakaze.Sdk.Csf;

namespace Shimakaze.Sdk.Engine.Csf;

/// <summary>
/// CSF 合并工具。
/// </summary>
/// <remarks>
/// 将多个 CSF 数据合并为一个：标签按名称（忽略大小写）合并，后出现的源
/// 覆盖先出现的同名标签；最终输出保持首次出现的相对顺序。
/// </remarks>
public static class CsfMerger
{
    /// <summary>
    /// 合并多个 CSF 数据。
    /// </summary>
    /// <param name="sources">按优先级从低到高排列的 CSF 数据（后出现的覆盖先出现的同名标签，标签名称忽略大小写）。</param>
    /// <returns>合并后的 CSF 数据。</returns>
    public static CsfData Merge(params IEnumerable<CsfData> sources)
    {
        CsfMetadata metadata = new();
        {
            var tmp = sources.First().Metadata;
            metadata.Version = tmp.Version;
            metadata.Language = tmp.Language;
        }
        foreach (var item in sources.Skip(1).Select(i => i.Metadata))
        {
            if (metadata.Version != item.Version)
                throw new NotSupportedException();
            if (metadata.Language != item.Language)
                throw new NotSupportedException();
        }

        Dictionary<string, CsfLabel> csf = new(StringComparer.OrdinalIgnoreCase);
        foreach (var label in sources.SelectMany(i => i))
            csf[label.Name] = label;

        CsfData result = new(metadata, [.. csf.Values]);
        result.UpdateMetadataCount();
        return result;
    }
}
