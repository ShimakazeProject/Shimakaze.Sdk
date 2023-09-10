using System.Collections;
using System.Text;

using Microsoft.Extensions.Primitives;

namespace Shimakaze.Sdk.Map.Trigger;

/// <summary>
/// 触发的行为
/// </summary>
/// <param name="Count"></param>
/// <param name="Items"></param>
public sealed record class TriggerAction(
    int Count,
    IList<TriggerActionItem> Items
) : IList<TriggerActionItem>
{
    /// <inheritdoc/>
    public TriggerActionItem this[int index] { get => Items[index]; set => Items[index] = value; }

    /// <inheritdoc/>
    public bool IsReadOnly => Items.IsReadOnly;

    private bool PrintMembers(StringBuilder builder)
    {
        builder.AppendLine($"Count = {Count}, Items = [");
        foreach (var item in Items)
            builder
                .Append("    ")
                .Append(item.ToString())
                .AppendLine(",");

        builder.Append(']');
        return true;
    }

    internal static TriggerAction Parse(string str)
    {
        var tmp = str.Split(',');
        TriggerAction data = new(
            int.Parse(tmp[0]),
            new List<TriggerActionItem>()
        );
        for (int i = 1; i < tmp.Length;)
        {
            int e = int.Parse(tmp[i++]);
            string p1 = tmp[i++];
            string p2 = tmp[i++];
            string p3 = tmp[i++];
            string p4 = tmp[i++];
            string p5 = tmp[i++];
            string p6 = tmp[i++];
            string p7 = tmp[i++];
            data.Add(new(e, p1, p2, p3, p4, p5, p6, p7));
        }
        return data;
    }

    internal string ToIniValue() => new StringBuilder()
        .Append(Count)
        .Append(',')
        .Append(string.Join(',', Items.Select(x => x.ToIniValue())))
        .ToString();

    /// <inheritdoc/>
    public void Add(TriggerActionItem item)
    {
        Items.Add(item);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        Items.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(TriggerActionItem item)
    {
        return Items.Contains(item);
    }

    /// <inheritdoc/>
    public void CopyTo(TriggerActionItem[] array, int arrayIndex)
    {
        Items.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public IEnumerator<TriggerActionItem> GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    /// <inheritdoc/>
    public int IndexOf(TriggerActionItem item)
    {
        return Items.IndexOf(item);
    }

    /// <inheritdoc/>
    public void Insert(int index, TriggerActionItem item)
    {
        Items.Insert(index, item);
    }

    /// <inheritdoc/>
    public bool Remove(TriggerActionItem item)
    {
        return Items.Remove(item);
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
        Items.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Items).GetEnumerator();
    }
}
