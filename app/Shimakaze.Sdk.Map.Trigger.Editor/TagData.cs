namespace Shimakaze.Sdk.Map.Trigger;

public sealed record class TagData(TagPersistence Persistence, string Name, string TriggerId)
{
    public static TagData Parse(string str)
    {
        var tmp = str.Split(',');
        return new(
            (TagPersistence)int.Parse(tmp[0]),
            tmp[1],
            tmp[2]
        );
    }
}
