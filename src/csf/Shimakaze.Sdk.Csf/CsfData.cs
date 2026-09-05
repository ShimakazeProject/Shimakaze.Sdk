using System.Collections;

namespace Shimakaze.Sdk.Csf;

/// <summary>
/// 表示一个 CSF 文件
/// </summary>
public record class CsfData(CsfMetadata Metadata, List<CsfLabel> Labels) : IList<CsfLabel>
{
    /// <summary>
    /// 初始化一个新的 CSF 文件实例
    /// </summary>
    public CsfData() : this(new(), [])
    {
    }

    /// <summary>
    /// 初始化一个新的 CSF 文件实例
    /// </summary>
    /// <param name="metadata"></param>
    public CsfData(CsfMetadata metadata) : this(metadata, new(metadata.LabelCount))
    {
    }

    /// <inheritdoc/>
    public CsfLabel this[int index]
    {
        get => Labels[index];
        set => Labels[index] = value;
    }

    /// <summary>
    /// Gets or sets metadata.
    /// </summary>
    public CsfMetadata Metadata { get; set; } = Metadata;

    /// <inheritdoc/>
    public int Count => Labels.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    /// <remarks>
    /// 此方法会修改元数据，若不希望被修改元数据请直接操作 <see cref="Labels" />
    /// </remarks>
    public virtual void Add(CsfLabel item)
    {
        Metadata.Modify((ref i) =>
        {
            i.LabelCount++;
            i.StringCount += item.Count;
        });
        Labels.Add(item);
    }

    /// <summary>
    /// 添加一个标签
    /// </summary>
    /// <remarks>
    /// 此方法会修改元数据，若不希望被修改元数据请直接操作 <see cref="Labels" />
    /// </remarks>
    /// <param name="label"></param>
    /// <param name="values"></param>
    public void Add(string label, params ReadOnlySpan<string> values)
    {
        CsfLabel data = new(label, values.Length);

        foreach (string value in values)
            data.Add(new(value, default));

        Add(data);
    }

    /// <summary>
    /// 添加一个标签
    /// </summary>
    /// <remarks>
    /// 此方法会修改元数据，若不希望被修改元数据请直接操作 <see cref="Labels" />
    /// </remarks>
    /// <param name="label"></param>
    /// <param name="values"></param>
    public void Add(string label, params CsfValue[] values)
    {
        CsfLabel data = new(label, [.. values]);

        Add(data);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        Metadata.Modify((ref i) =>
        {
            i.LabelCount = 0;
            i.StringCount = 0;
        });
        Labels.Clear();
    }


    /// <inheritdoc/>
    public bool Contains(CsfLabel item) => Labels.Contains(item);


    /// <inheritdoc/>
    public void CopyTo(CsfLabel[] array, int arrayIndex) => Labels.CopyTo(array, arrayIndex);


    /// <inheritdoc/>
    public IEnumerator<CsfLabel> GetEnumerator() => Labels.GetEnumerator();


    /// <inheritdoc/>
    public int IndexOf(CsfLabel item) => Labels.IndexOf(item);


    /// <inheritdoc/>
    /// <remarks>
    /// 此方法会修改元数据，若不希望被修改元数据请直接操作 <see cref="Labels" />
    /// </remarks>
    public void Insert(int index, CsfLabel item)
    {
        Metadata.Modify((ref i) =>
        {
            i.LabelCount++;
            i.StringCount += item.Count;
        });
        Labels.Insert(index, item);
    }


    /// <inheritdoc/>
    /// <remarks>
    /// 此方法会修改元数据，若不希望被修改元数据请直接操作 <see cref="Labels" />
    /// </remarks>
    public bool Remove(CsfLabel item)
    {
        Metadata.Modify((ref i) =>
        {
            i.LabelCount--;
            i.StringCount -= item.Count;
        });
        return Labels.Remove(item);
    }


    /// <inheritdoc/>
    /// <remarks>
    /// 此方法会修改元数据，若不希望被修改元数据请直接操作 <see cref="Labels" />
    /// </remarks>
    public void RemoveAt(int index)
    {
        Metadata.Modify((ref i) =>
        {
            i.LabelCount--;
            i.StringCount -= Labels[index].Count;
        });
        Labels.RemoveAt(index);
    }

    /// <summary>
    /// 更新文件头数据
    /// </summary>
    public void UpdateMetadataCount() => Metadata.Modify((ref i) =>
    {
        i.LabelCount = Labels.Count;
        i.StringCount = Labels.Sum(x => x.Count);
    });

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
