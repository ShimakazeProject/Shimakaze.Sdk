namespace Shimakaze.Sdk.Shp.Maker;

internal sealed record class SequenceData(int Start, int Count, string Angle)
{
    public bool HasAngle => int.TryParse(Angle, out _);
    public int AngleCount => int.TryParse(Angle, out var result) ? result : 1;
    public int End { get; set; } = Count;
}
