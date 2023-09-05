namespace Shimakaze.Sdk.Map.Trigger;

/// <summary>
/// 触发器
/// </summary>
/// <param name="House"></param>
/// <param name="LinkedTrigger"></param>
/// <param name="Name"></param>
/// <param name="Disable"></param>
/// <param name="Easy"></param>
/// <param name="Normal"></param>
/// <param name="Hard"></param>
/// <param name="Persistence"></param>
public sealed record class Trigger(
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
    internal static Trigger Parse(string str)
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
