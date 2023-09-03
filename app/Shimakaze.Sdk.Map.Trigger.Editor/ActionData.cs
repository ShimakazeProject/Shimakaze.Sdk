using System.Collections;
using System.Text;

namespace Shimakaze.Sdk.Map.Trigger;

public sealed record class ActionData(
    int Count,
    IList<ActionDataItem> Items
) : IList<ActionDataItem>
{
    public ActionDataItem this[int index] { get => Items[index]; set => Items[index] = value; }

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
    public static ActionData Parse(string str)
    {
        var tmp = str.Split(',');
        ActionData data = new(
            int.Parse(tmp[0]),
            new List<ActionDataItem>()
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

    public void Add(ActionDataItem item)
    {
        Items.Add(item);
    }

    public void Clear()
    {
        Items.Clear();
    }

    public bool Contains(ActionDataItem item)
    {
        return Items.Contains(item);
    }

    public void CopyTo(ActionDataItem[] array, int arrayIndex)
    {
        Items.CopyTo(array, arrayIndex);
    }

    public IEnumerator<ActionDataItem> GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    public int IndexOf(ActionDataItem item)
    {
        return Items.IndexOf(item);
    }

    public void Insert(int index, ActionDataItem item)
    {
        Items.Insert(index, item);
    }

    public bool Remove(ActionDataItem item)
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
