namespace Shimakaze.Sdk.Shp.Maker;

internal sealed record class SequenceData(int Start, int Count, string Angle)
{
    public bool HasAngle => Angle is "8";
    public int End { get; set; } = Count;
}
