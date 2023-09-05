using System.Collections;
using System.Text;

namespace Shimakaze.Sdk.Map.Trigger;

/// <summary>
/// 触发的事件
/// </summary>
/// <param name="Count"></param>
/// <param name="Items"></param>
public sealed record class TriggerEvent(
    int Count,
    IList<TriggerEventItem> Items
) : IList<TriggerEventItem>
{
    /// <inheritdoc/>
    public TriggerEventItem this[int index] { get => Items[index]; set => Items[index] = value; }

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

    internal static TriggerEvent Parse(string str)
    {
        var tmp = str.Split(',');
        TriggerEvent data = new(
            int.Parse(tmp[0]),
            new List<TriggerEventItem>()
        );
        for (int i = 1; i < tmp.Length;)
        {
            int e = int.Parse(tmp[i++]);
            int p1 = int.Parse(tmp[i++]);
            int p2 = int.Parse(tmp[i++]);
            if (i < tmp.Length && !int.TryParse(tmp[i], out _))
                data.Add(new(e, p1, p2, tmp[i++]));
            else
                data.Add(new(e, p1, p2));
        }
        return data;
    }

    /// <inheritdoc/>
    public void Add(TriggerEventItem item)
    {
        Items.Add(item);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        Items.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(TriggerEventItem item)
    {
        return Items.Contains(item);
    }

    /// <inheritdoc/>
    public void CopyTo(TriggerEventItem[] array, int arrayIndex)
    {
        Items.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public IEnumerator<TriggerEventItem> GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    /// <inheritdoc/>
    public int IndexOf(TriggerEventItem item)
    {
        return Items.IndexOf(item);
    }

    /// <inheritdoc/>
    public void Insert(int index, TriggerEventItem item)
    {
        Items.Insert(index, item);
    }

    /// <inheritdoc/>
    public bool Remove(TriggerEventItem item)
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
