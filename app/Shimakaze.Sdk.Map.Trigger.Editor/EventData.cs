using System.Collections;
using System.Text;

namespace Shimakaze.Sdk.Map.Trigger;

public sealed record class EventData(
    int Count,
    IList<EventDataItem> Items
) : IList<EventDataItem>
{
    public EventDataItem this[int index] { get => Items[index]; set => Items[index] = value; }

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

    public static EventData Parse(string str)
    {
        var tmp = str.Split(',');
        EventData data = new(
            int.Parse(tmp[0]),
            new List<EventDataItem>()
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

    public void Add(EventDataItem item)
    {
        Items.Add(item);
    }

    public void Clear()
    {
        Items.Clear();
    }

    public bool Contains(EventDataItem item)
    {
        return Items.Contains(item);
    }

    public void CopyTo(EventDataItem[] array, int arrayIndex)
    {
        Items.CopyTo(array, arrayIndex);
    }

    public IEnumerator<EventDataItem> GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    public int IndexOf(EventDataItem item)
    {
        return Items.IndexOf(item);
    }

    public void Insert(int index, EventDataItem item)
    {
        Items.Insert(index, item);
    }

    public bool Remove(EventDataItem item)
    {
        return Items.Remove(item);
    }

    public void RemoveAt(int index)
    {
        Items.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Items).GetEnumerator();
    }
}
