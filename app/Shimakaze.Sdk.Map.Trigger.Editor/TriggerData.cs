namespace Shimakaze.Sdk.Map.Trigger;

public sealed record class TriggerData(
    string House,
    string LinkedTrigger,
    string Name,
    bool Disable,
    bool Easy,
    bool Normal,
    bool Hard,
    TriggerPersistence Persistence
)
{
    public static TriggerData Parse(string str)
    {
        var tmp = str.Split(',');
        return new(
            tmp[0],
            tmp[1],
            tmp[2],
            tmp[3] is "1" || (tmp[3] is "0" ? false : throw new FormatException()),
            tmp[4] is "1" || (tmp[4] is "0" ? false : throw new FormatException()),
            tmp[5] is "1" || (tmp[5] is "0" ? false : throw new FormatException()),
            tmp[6] is "1" || (tmp[6] is "0" ? false : throw new FormatException()),
            (TriggerPersistence)int.Parse(tmp[7])
        );
    }
}
