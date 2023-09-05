namespace Shimakaze.Sdk.Map.Trigger;

/// <summary>
/// 标签数据
/// </summary>
/// <param name="Persistence"></param>
/// <param name="Name"></param>
/// <param name="TriggerId"></param>
public sealed record class Tag(
    TagPersistence Persistence,
    string Name,
    string TriggerId)
{
    internal static Tag Parse(string str)
    {
        var tmp = str.Split(',');
        return new(
            (TagPersistence)int.Parse(tmp[0]),
            tmp[1],
            tmp[2]
        );
    }
}
